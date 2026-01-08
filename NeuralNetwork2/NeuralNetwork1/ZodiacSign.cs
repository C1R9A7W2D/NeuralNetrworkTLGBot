using System;
using System.Collections.Generic;

namespace NeuralNetwork1
{
    public enum ZodiacSign : byte
    {
        Aries = 0, Taurus, Gemini, Cancer, Leo, Virgo,
        Libra, Scorpio, Sagittarius, Capricorn, Aquarius, Pisces, Undef
    }

    public static class ZodiacSignHelper
    {
        private static readonly Dictionary<ZodiacSign, string> RussianNames = new Dictionary<ZodiacSign, string>
        {
            { ZodiacSign.Aries, "Овен" },
            { ZodiacSign.Taurus, "Телец" },
            { ZodiacSign.Gemini, "Близнецы" },
            { ZodiacSign.Cancer, "Рак" },
            { ZodiacSign.Leo, "Лев" },
            { ZodiacSign.Virgo, "Дева" },
            { ZodiacSign.Libra, "Весы" },
            { ZodiacSign.Scorpio, "Скорпион" },
            { ZodiacSign.Sagittarius, "Стрелец" },
            { ZodiacSign.Capricorn, "Козерог" },
            { ZodiacSign.Aquarius, "Водолей" },
            { ZodiacSign.Pisces, "Рыбы" },
            { ZodiacSign.Undef, "Не определен" }
        };

        private static readonly Dictionary<string, ZodiacSign> FromRussian = new Dictionary<string, ZodiacSign>
        {
            { "Овен", ZodiacSign.Aries },
            { "Телец", ZodiacSign.Taurus },
            { "Близнецы", ZodiacSign.Gemini },
            { "Рак", ZodiacSign.Cancer },
            { "Лев", ZodiacSign.Leo },
            { "Дева", ZodiacSign.Virgo },
            { "Весы", ZodiacSign.Libra },
            { "Скорпион", ZodiacSign.Scorpio },
            { "Стрелец", ZodiacSign.Sagittarius },
            { "Козерог", ZodiacSign.Capricorn },
            { "Водолей", ZodiacSign.Aquarius },
            { "Рыбы", ZodiacSign.Pisces },
            { "Не определен", ZodiacSign.Undef }
        };

        public static string ToRussianString(this ZodiacSign sign)
        {
            return RussianNames.ContainsKey(sign) ? RussianNames[sign] : sign.ToString();
        }

        public static ZodiacSign FromRussianString(string russianName)
        {
            return FromRussian.ContainsKey(russianName) ? FromRussian[russianName] : ZodiacSign.Undef;
        }

        public static string[] GetAllRussianNames()
        {
            var names = new string[12];
            for (int i = 0; i < 12; i++)
            {
                names[i] = RussianNames[(ZodiacSign)i];
            }
            return names;
        }
    }
}