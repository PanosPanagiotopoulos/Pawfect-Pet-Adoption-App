using System.Numerics;

namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Embedding
{
    public class WeightedChunk<T> where T : INumber<T>
    {
        public T[] Embedding { get; set; }
        public Double Weight { get; set; }
        public Int32 TextLength { get; set; }
        public Int32 Position { get; set; }
        public Int32 StartIndex { get; set; }
        public Int32 EndIndex { get; set; }
    }
}
