namespace Game.Actor.Shape.Interface
{
    using System.Runtime.Serialization;

    [DataContract]
    public class Shape
    {
        [DataMember]
        public double X { get; set; }

        [DataMember]
        public double Y { get; set; }

        [DataMember]
        public double DiffX { get; set; }

        [DataMember]
        public double DiffY { get; set; }

        [DataMember]
        public double Angle { get; set; }
    }
}
