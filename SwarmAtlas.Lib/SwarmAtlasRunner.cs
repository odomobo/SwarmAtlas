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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SwarmAtlas.Lib
{
    public class RawInitData
    {
        public byte[] GameInfo { get; set; }
        public byte[] Data { get; set; }
        public byte[] PingResponse { get; set; }
        public byte[] Observation { get; set; }
        public uint PlayerId { get; set; }

        public InitData GetInitData(ProtobufProxy proxy)
        {
            return new InitData {
                GameInfo = proxy.GetResponseFromResponseBuf(GameInfo).GameInfo,
                Data = proxy.GetResponseFromResponseBuf(Data).Data,
                PingResponse = proxy.GetResponseFromResponseBuf(PingResponse).Ping,
                Observation = proxy.GetResponseFromResponseBuf(Observation).Observation,
                PlayerId = PlayerId,
            };
        }
    }

    public class InitData
    {
        public ResponseGameInfo GameInfo { get; set; }
        public ResponseData Data { get; set; }
        public ResponsePing PingResponse { get; set; }
        public ResponseObservation Observation { get; set; }
        public uint PlayerId { get; set; }
    }

    public class RawFrameData
    {
        public byte[] Observation { get; set; }
        public int FrameNumber { get; set; }

        public FrameData GetFrameData(ProtobufProxy proxy)
        {
            return new FrameData
            {
                Observation = proxy.GetResponseFromResponseBuf(Observation).Observation,
                FrameNumber = FrameNumber,
            };
        }
    }

    public class FrameData
    {
        public ResponseObservation Observation { get; set;}
        public int FrameNumber { get; set; }
    }

    public class SwarmAtlasRunner : IBot
    {
        public string BotName => "SwarmAtlas";

        private LiteDatabase _liteDb = null;
        private readonly GameConfig _gameConfig;
        private readonly SwarmAtlas _swarmAtlas;

        public SwarmAtlasRunner(GameConfig gameConfig, SwarmAtlas swarmAtlas)
        {
            _gameConfig = gameConfig;
            _swarmAtlas = swarmAtlas;
        }

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

                // on first loop, get db and set init data
                if (_liteDb == null)
                {
                    var rawInitData = new RawInitData
                    {
                        GameInfo = gameInfoResponseBuf,
                        Data = dataResponseBuf,
                        PingResponse = pingResponseBuf,
                        Observation = observationResponseBuf,
                        PlayerId = playerId,
                    };

                    var dbFilename = GenerateDbFilename();
                    OpenDb(dbFilename);
                    var initDataCollection = _liteDb.GetCollection<RawInitData>("rawInitData");
                    initDataCollection.Insert(rawInitData);

                    _swarmAtlas.Init(rawInitData.GetInitData(proxy));
                }

                var frameData = new RawFrameData
                {
                    FrameNumber = frameNumber,
                    Observation = observationResponseBuf,
                };
                var frameDataCollection = _liteDb.GetCollection<RawFrameData>("rawFrameData");
                frameDataCollection.EnsureIndex(x => x.FrameNumber);
                frameDataCollection.Insert(frameData);
                _liteDb.Commit();

                var actions = new Queue<Action>();
                _swarmAtlas.OnFrame(frameData.GetFrameData(proxy), actions);

                if (actions.Any())
                {
                    Request actionRequest = new Request();
                    actionRequest.Action = new RequestAction();
                    actionRequest.Action.Actions.AddRange(actions);
                    await proxy.SendRequest(actionRequest);
                }

                // TODO: not sure if this is necessary, or even does anything. I guess it's good in case we ever aren't running in realtime, although the Sleep() will cause issues
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
            var rawInitDataCollection = _liteDb.GetCollection<RawInitData>("rawInitData");
            var rawInitData = rawInitDataCollection.FindOne(x => true);
            var initData = rawInitData.GetInitData(proxy);
            _swarmAtlas.Init(initData);

            for (int frameNumber = 0; ; frameNumber++)
            {
                var rawFrameDataCollection = _liteDb.GetCollection<RawFrameData>("rawFrameData");
                var rawFrameDatas = rawFrameDataCollection.Find(x => x.FrameNumber == frameNumber);
                // this means we reached the end of the playback
                if (!rawFrameDatas.Any())
                {
                    CloseDb(); // TODO: probably make this a finally?
                    break;
                }

                var actions = new Queue<Action>();
                var frame = rawFrameDatas.Single().GetFrameData(proxy);
                _swarmAtlas.OnFrame(frame, actions);
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
