using System;
using OgameBot.Objects.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgameBot.Objects
{
    [ComplexType]
    public class Resources : IEquatable<Resources>
    {
        public Resources()
        {
        }

        public Resources(int metal, int crystal, int deuterium, int energy)
        {
            Metal = metal;
            Crystal = crystal;
            Deuterium = deuterium;
            Energy = energy;
        }

        public Resources(int metal, int crystal, int deuterium) : this(metal, crystal, deuterium, 0)
        {
        }

        public bool Equals(Resources other)
        {
            return Metal == other.Metal && Crystal == other.Crystal && Deuterium == other.Deuterium && Energy == other.Energy;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Resources && Equals((Resources)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Metal;
                hashCode = (hashCode * 397) ^ Crystal;
                hashCode = (hashCode * 397) ^ Deuterium;
                hashCode = (hashCode * 397) ^ Energy;
                return hashCode;
            }
        }

        public static bool operator ==(Resources left, Resources right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Resources left, Resources right)
        {
            return !left.Equals(right);
        }

        public int Metal { get; set; }

        public int Crystal { get; set; }

        public int Deuterium { get; set; }

        public int Energy { get; set; }

        /// <summary>
        /// Total transferrable (Excludes energy)
        /// </summary>
        [NotMapped]
        public int Total => Metal + Crystal + Deuterium;

        public int GetResource(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Metal:
                    return Metal;
                case ResourceType.Crystal:
                    return Crystal;
                case ResourceType.Deuterium:
                    return Deuterium;
                case ResourceType.Energy:
                    return Energy;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public override string ToString()
        {
            if (Energy > 0)
                return $"M: {Metal:N0}, C: {Crystal:N0}, D: {Deuterium:N0}, E: {Energy:N0}";

            return $"M: {Metal:N0}, C: {Crystal:N0}, D: {Deuterium:N0}";
        }
    }
}