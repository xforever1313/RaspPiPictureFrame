//
// PiPictureFrame - Digital Picture Frame built for the Raspberry Pi.
// Copyright (C) 2022 Seth Hendrick
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

namespace PiPictureFrame.Web
{
    public enum Hour
    {
        Hour_0 = 0,
        Hour_1 = 1,
        Hour_2 = 2,
        Hour_3 = 3,
        Hour_4 = 4,
        Hour_5 = 5,
        Hour_6 = 6,
        Hour_7 = 7,
        Hour_8 = 8,
        Hour_9 = 9,
        Hour_10 = 10,
        Hour_11 = 11,
        Hour_12 = 12,
        Hour_13 = 13,
        Hour_14 = 14,
        Hour_15 = 15,
        Hour_16 = 16,
        Hour_17 = 17,
        Hour_18 = 18,
        Hour_19 = 19,
        Hour_20 = 20,
        Hour_21 = 21,
        Hour_22 = 22,
        Hour_23 = 23,
    }

    public enum Minute
    {
        Minute_0 = 0,
        Minute_1 = 1,
        Minute_2 = 2,
        Minute_3 = 3,
        Minute_4 = 4,
        Minute_5 = 5,
        Minute_6 = 6,
        Minute_7 = 7,
        Minute_8 = 8,
        Minute_9 = 9,
        Minute_10 = 10,
        Minute_11 = 11,
        Minute_12 = 12,
        Minute_13 = 13,
        Minute_14 = 14,
        Minute_15 = 15,
        Minute_16 = 16,
        Minute_17 = 17,
        Minute_18 = 18,
        Minute_19 = 19,
        Minute_20 = 20,
        Minute_21 = 21,
        Minute_22 = 22,
        Minute_23 = 23,
        Minute_24 = 24,
        Minute_25 = 25,
        Minute_26 = 26,
        Minute_27 = 27,
        Minute_28 = 28,
        Minute_29 = 29,
        Minute_30 = 30,
        Minute_31 = 31,
        Minute_32 = 32,
        Minute_33 = 33,
        Minute_34 = 34,
        Minute_35 = 35,
        Minute_36 = 36,
        Minute_37 = 37,
        Minute_38 = 38,
        Minute_39 = 39,
        Minute_40 = 40,
        Minute_41 = 41,
        Minute_42 = 42,
        Minute_43 = 43,
        Minute_44 = 44,
        Minute_45 = 45,
        Minute_46 = 46,
        Minute_47 = 47,
        Minute_48 = 48,
        Minute_49 = 49,
        Minute_50 = 50,
        Minute_51 = 51,
        Minute_52 = 52,
        Minute_53 = 53,
        Minute_54 = 54,
        Minute_55 = 55,
        Minute_56 = 56,
        Minute_57 = 57,
        Minute_58 = 58,
        Minute_59 = 59
    }

    public enum Brightness : byte
    {
        Brightness_0 = 0,
        Brightness_5 = 5,
        Brightness_10 = 10,
        Brightness_15 = 15,
        Brightness_20 = 20,
        Brightness_25 = 25,
        Brightness_30 = 30,
        Brightness_35 = 35,
        Brightness_40 = 40,
        Brightness_45 = 45,
        Brightness_50 = 50,
        Brightness_55 = 55,
        Brightness_60 = 60,
        Brightness_65 = 65,
        Brightness_70 = 70,
        Brightness_75 = 75,
        Brightness_80 = 80,
        Brightness_85 = 85,
        Brightness_90 = 90,
        Brightness_95 = 95,
        Brightness_100 = 100
    }

    public enum ChangeIntervals
    {
        Seconds_15 = 15,

        Seconds_30 = 30,

        Seconds_45 = 45,

        Minutes_1 = 60,

        Minutes_2 = 60 * 2,

        Minutes_3 = 60 * 3,

        Minutes_5 = 60 * 5,

        Minutes_10 = 60 * 10,

        Minutes_15 = 60 * 15,

        Minutes_20 = 60 * 20,

        Minutes_30 = 60 * 30,

        Minutes_45 = 60 * 45,

        Hour_1 = 60 * 60
    }

    public static class Enums
    {
        public static TEnum? TryConvertEnum<TEnum>( int enumValue )
            where TEnum : struct, Enum
        {
            foreach( TEnum e in Enum.GetValues<TEnum>() )
            {
                int eAsInt = Convert.ToInt32( e );
                if( eAsInt == enumValue )
                {
                    return e;
                }
            }

            return null;
        }
    }
}
