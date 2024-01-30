using System.Collections.Generic;
using SC2APIProtocol;
using System;

using Action = SC2APIProtocol.Action;
using System.Linq;
using LiteDB;
using SC2API.CSharp;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;

namespace ExampleBot
{
    class UnitData
    {
        public List<Unit> Units { get; set; }
    }

    class InitData
    {
        public byte[] GameInfo { get; set; }
        public byte[] Data { get; set; }
        public byte[] PingResponse { get; set; }
        public byte[] Observation { get; set; }
        public uint PlayerId { get; set; }
    }

    class FrameData
    {
        public byte[] Observation { get; set; }
        public int FrameNumber { get; set; }
    }

    class ExampleBot : IBot
    {
        private readonly GameConfig _gameConfig;
        internal ExampleBot(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        public string BotName => "ExampleBot";

        private LiteDatabase _liteDb = null;
        private InnerBot _innerBot = null;

        public async Task Run(ProtobufProxy proxy, uint playerId)
        {
            Request gameInfoReq = new Request();
            gameInfoReq.GameInfo = new RequestGameInfo();
            var (gameInfoResponse, gameInfoResponseBuf) = await proxy.SendRequestRaw(gameInfoReq);

            Request gameDataRequest = new Request();
            gameDataRequest.Data = new RequestData();
            gameDataRequest.Data.UnitTypeId = true;
            gameDataRequest.Data.AbilityId = true;
            gameDataRequest.Data.BuffId = true;
            gameDataRequest.Data.EffectId = true;
            gameDataRequest.Data.UpgradeId = true;
            var (dataResponse, dataResponseBuf) = await proxy.SendRequestRaw(gameDataRequest);
            dataResponse = proxy.GetResponseFromResponseBuf(dataResponseBuf);

            Request pingRequest = new Request();
            pingRequest.Ping = new RequestPing();
            var (pingResponse, pingResponseBuf) = await proxy.SendRequestRaw(pingRequest);

            for (int frameNumber = 0; ; frameNumber++)
            {

                Request observationRequest = new Request();
                observationRequest.Observation = new RequestObservation();
                var (observationResponse, observationResponseBuf) = await proxy.SendRequestRaw(observationRequest);

                if (observationResponse.Observation == null 
                    || observationResponse.Status == Status.Ended
                    || observationResponse.Status == Status.Quit)
                {
                    // game is over
                    CloseDb(); // TODO: probably make this a finally?
                    break;
                }

                // on first loop, create the inner bot... not sure if we need observation data for init
                if (_innerBot == null)
                {
                    var initData = new InitData
                    {
                        GameInfo = gameInfoResponseBuf,
                        Data = dataResponseBuf,
                        PingResponse = pingResponseBuf,
                        Observation = observationResponseBuf,
                        PlayerId = playerId,
                    };

                    var dbFilename = GenerateDbFilename();
                    OpenDb(dbFilename);
                    var initDataCollection = _liteDb.GetCollection<InitData>("initData");
                    initDataCollection.Insert(initData);

                    _innerBot = new InnerBot(proxy, initData);
                }

                var frameData = new FrameData
                {
                    FrameNumber = frameNumber,
                    Observation = observationResponseBuf,
                };
                var frameDataCollection = _liteDb.GetCollection<FrameData>("frameData");
                frameDataCollection.EnsureIndex(x => x.FrameNumber);
                frameDataCollection.Insert(frameData);
                _liteDb.Commit();

                var actions = _innerBot.OnFrame(frameData);

                if (actions.Any())
                {
                    Request actionRequest = new Request();
                    actionRequest.Action = new RequestAction();
                    actionRequest.Action.Actions.AddRange(actions);
                    await proxy.SendRequest(actionRequest);
                }

                // not sure if this is necessary, or even does anything. I guess it's good in case we ever aren't running in realtime, although the Sleep() will cause issues
                Request stepRequest = new Request();
                stepRequest.Step = new RequestStep();
                stepRequest.Step.Count = 1;
                await proxy.SendRequest(stepRequest);

                Thread.Sleep(100);
            }
        }

        // TODO: can make this into a generator to allow stepping through, by some other method
        public void Simulate(ProtobufProxy proxy, string dbFilename)
        {
            OpenDb(dbFilename);
            var initDataCollection = _liteDb.GetCollection<InitData>("initData");
            var initData = initDataCollection.FindOne(x => true);
            _innerBot = new InnerBot(proxy, initData);

            for (int frameNumber = 0; ; frameNumber++)
            {
                var frameDataCollection = _liteDb.GetCollection<FrameData>("frameData");
                var frameDatas = frameDataCollection.Find(x => x.FrameNumber == frameNumber);
                // this means we reached the end of the playback
                if (!frameDatas.Any())
                {
                    CloseDb(); // TODO: probably make this a finally?
                    break;
                }

                _innerBot.OnFrame(frameDatas.First());
            }
        }

        private string GenerateDbFilename()
        {
            return $"match {DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db";
        }

        // TODO: remove
        public async Task<ResponsePing> Ping(ProtobufProxy proxy)
        {
            Request request = new Request();
            request.Ping = new RequestPing();
            Response response = await proxy.SendRequest(request);
            return response.Ping;
        }

        private void OpenDb(string dbFilename)
        {
            // TODO: use a different path; probably one just for the databases
            _liteDb = new LiteDatabase(Path.Combine(_gameConfig.ReplayPath, dbFilename));
        }

        private void CloseDb()
        {
            if (_liteDb != null)
            {
                _liteDb.Dispose();
                _liteDb = null;
            }
        }
    }
}
