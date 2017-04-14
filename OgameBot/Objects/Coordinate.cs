﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OgameBot.Objects.Types;
using Newtonsoft.Json;

namespace OgameBot.Objects
{
    public struct Coordinate : IEquatable<Coordinate>, IComparer<Coordinate>
    {
        private static readonly Regex ParseRegex = new Regex(@"([\d]+):([\d]+):([\d]+)", RegexOptions.Compiled);

        public byte Galaxy { get; set; }

        public short System { get; set; }

        public byte Planet { get; set; }

        public CoordinateType Type { get; set; }

        public Coordinate(byte galaxy, short system, byte planet, CoordinateType type)
        {
            Galaxy = galaxy;
            System = system;
            Planet = planet;
            Type = type;
        }

        public static Coordinate Create(Coordinate baseCoordinate, CoordinateType type)
        {
            return new Coordinate(baseCoordinate.Galaxy, baseCoordinate.System, baseCoordinate.Planet, type);
        }

        public static Coordinate Create(SystemCoordinate baseCoordinate, byte planet, CoordinateType type)
        {
            return new Coordinate(baseCoordinate.Galaxy, baseCoordinate.System, planet, type);
        }

        public static Coordinate Create(Coordinate baseCoordinate, byte planet, CoordinateType type)
        {
            return new Coordinate(baseCoordinate.Galaxy, baseCoordinate.System, planet, type);
        }

        public static Coordinate Create(SystemCoordinate baseCoordinate, short system, byte planet, CoordinateType type)
        {
            return new Coordinate(baseCoordinate.Galaxy, system, planet, type);
        }

        public static Coordinate Create(Coordinate baseCoordinate, short system, byte planet, CoordinateType type)
        {
            return new Coordinate(baseCoordinate.Galaxy, system, planet, type);
        }

        public static Coordinate Parse(string input, CoordinateType type)
        {
            Match match = ParseRegex.Match(input);

            if (!match.Success)
                throw new ArgumentException("Unable to find a coordinate", nameof(input));

            return new Coordinate
            {
                Galaxy = Convert.ToByte(match.Groups[1].Value),
                System = Convert.ToInt16(match.Groups[2].Value),
                Planet = Convert.ToByte(match.Groups[3].Value),
                Type = type
            };
        }

        public int DistanceTo(Coordinate other)
        {
            // http://ogame.wikia.com/wiki/Distance
            if (other.Galaxy != Galaxy)
                return 20000 * Math.Abs(other.Galaxy - Galaxy);

            if (other.System != System)
                return 2700 + 95 * Math.Abs(other.System - System);

            return 1000 + 5 * Math.Abs(other.Planet - Planet);
        }

        public bool Equals(Coordinate other)
        {
            return Galaxy == other.Galaxy && System == other.System && Planet == other.Planet && Type == other.Type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Galaxy.GetHashCode();
                hashCode = (hashCode * 397) ^ System.GetHashCode();
                hashCode = (hashCode * 397) ^ Planet.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Type;
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Coordinate && Equals((Coordinate)obj);
        }

        [JsonIgnore]
        public long Id => this;

        public static implicit operator long(Coordinate coord)
        {
            return CoordHelper.ToNumber(coord);
        }

        public static implicit operator Coordinate(long id)
        {
            return CoordHelper.GetCoordinate(id);
        }

        public static bool operator <(Coordinate left, Coordinate right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(Coordinate left, Coordinate right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator >=(Coordinate left, Coordinate right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(Coordinate left, Coordinate right)
        {
            return Compare(left, right) <= 0;
        }

        public static bool operator ==(Coordinate left, Coordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Coordinate left, Coordinate right)
        {
            return !left.Equals(right);
        }

        public static int Compare(Coordinate x, Coordinate y)
        {
            if (x.Galaxy != y.Galaxy)
                return x.Galaxy.CompareTo(y.Galaxy);

            if (x.System != y.System)
                return x.System.CompareTo(y.System);

            if (x.Planet != y.Planet)
                return x.Planet.CompareTo(y.Planet);

            return x.Type.CompareTo(y.Type);
        }

        int IComparer<Coordinate>.Compare(Coordinate x, Coordinate y)
        {
            return Compare(x, y);
        }

        public override string ToString()
        {
            if (Type == CoordinateType.Planet)
                return $"[{Galaxy}:{System}:{Planet}]";

            return $"[{Galaxy}:{System}:{Planet}] {Type}";
        }
    }
}