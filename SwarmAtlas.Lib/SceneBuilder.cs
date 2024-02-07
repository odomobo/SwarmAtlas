using Microsoft.Xna.Framework;
using SwarmAtlas.Gui;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmAtlas.Lib
{
    public class SceneBuilder
    {
        private Scene _scene;
        private InitData _initData;

        public required IRenderer Renderer { protected get; init; }
        public required Units Units { protected get; init; }

        public void Init(InitData initData)
        {
            _initData = initData;
        }

        public void OnFrame(FrameData frame, Queue<SC2APIProtocol.Action> actions)
        {
            _scene = new Scene();

            var rawData = frame.Observation.Observation.RawData;

            _scene.MapExtents = new Vector2(_initData.GameInfo.StartRaw.MapSize.X, _initData.GameInfo.StartRaw.MapSize.Y);

            _scene.MapObjects.Add(new Square(new Vector2(_initData.GameInfo.StartRaw.StartLocations[0].X, _initData.GameInfo.StartRaw.StartLocations[0].Y), 1, Color.Red));

            for (int index = 0; index < rawData.MapState.Creep.Size.X * rawData.MapState.Creep.Size.Y; index++)
            {
                var byteIndex = index / 8;
                var uncorrectedBitIndex = index % 8;
                var correctedBitIndex = 7 - uncorrectedBitIndex;
                var byteMask = 1 << correctedBitIndex;
                var maskedByteData = rawData.MapState.Creep.Data[byteIndex] & byteMask;

                if (maskedByteData != 0)
                {
                    var x = index % rawData.MapState.Creep.Size.X;
                    var y = index / rawData.MapState.Creep.Size.X;
                    _scene.MapObjects.Add(new Square(new Vector2(x, y), 1, Color.Purple));
                }
            }

        }

        // this is separated from onframe in case we need to add additional stuff to the scene in the future, for debugging purposes
        public void Render()
        {
            foreach (var larva in Units.MyLarva)
            {
                _scene.MapObjects.Add(
                    new Square(
                        new Vector2(larva.Pos.X, larva.Pos.Y), 
                        larva.Radius,
                        Color.Blue));
            }

            Renderer.UpdateScene(_scene);
        }
    }
}
