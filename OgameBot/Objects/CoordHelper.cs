﻿using System;
using Newtonsoft.Json;
using OgameBot.Objects.Types;
using Newtonsoft.Json.Linq;

namespace OgameBot.Objects
{
    public class CoordHelper : JsonConverter
    {
        public static long ToNumber(Coordinate coord)
        {
            // Gal  | Systm | Plnt | Type
            // byte | short | byte | byte

            return ((long)coord.Galaxy << 32) + (coord.System << 16) +
                   (coord.Planet << 8) + (byte)coord.Type;
        }

        public static int ToNumber(SystemCoordinate coord)
        {
            // Gal  | Systm
            // byte | short

            return (coord.Galaxy << 16) + coord.System;
        }

        public static bool IsSystemCoordinate(long id)
        {
            // If it's a long, it's a planet coordinate
            return id < int.MaxValue;
        }

        public static Coordinate GetCoordinate(long id)
        {
            byte gal = (byte)((id >> 32) & 0xFF);
            short sys = (short)((id >> 16) & 0xFFFF);
            byte pln = (byte)((id >> 8) & 0xFF);
            CoordinateType typ = (CoordinateType)(id & 0xFF);

            return new Coordinate(gal, sys, pln, typ);
        }

        public static SystemCoordinate GetSysCoordinate(int id)
        {
            byte gal = (byte)((id >> 16) & 0xFF);
            short sys = (short)(id & 0xFFFF);

            return new SystemCoordinate(gal, sys);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Coordinate coord = (Coordinate?)existingValue ?? new Coordinate();
            JObject obj = JObject.Load(reader);
            coord.Galaxy = obj["galaxy"].Value<byte>();
            coord.System = obj["system"].Value<short>();
            coord.Planet = obj["position"].Value<byte>();
            return coord;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}