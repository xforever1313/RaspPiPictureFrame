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

using PiPictureFrame.Api;

namespace PiPictureFrame.Web.Models
{
    public record SettingsModel(
        Hour AwakeHour,
        Minute AwakeMinute,
        bool DisableAwake,
        Hour SleepHour,
        Minute SleepMinute,
        bool DisableSleep,
        Brightness Brightness,
        ChangeIntervals ChangeInterval,
        string InfoMessage,
        string ErrorMessage
    ) : IAlert;

    public static class SettingsModelExtensions
    {
        public static PiPictureFrameConfig ToApiConfig( this SettingsModel model )
        {
            TimeOnly? awakeTime;
            if( model.DisableAwake )
            {
                awakeTime = null;
            }
            else
            {
                awakeTime = new TimeOnly( (int)model.AwakeHour, (int)model.AwakeMinute );
            }

            TimeOnly? sleepTime;
            if( model.DisableSleep )
            {
                sleepTime = null;
            }
            else
            {
                sleepTime = new TimeOnly( (int)model.SleepHour, (int)model.SleepMinute );
            }

            return new PiPictureFrameConfig
            {
                 AwakeTime = awakeTime,
                 Brightness = (byte)model.Brightness,
                 PhotoChangeInterval = TimeSpan.FromSeconds( (int) model.ChangeInterval ),
                 SleepTime = sleepTime
            };
        }

        public static SettingsModel FromApiConfig( this PiPictureFrameConfig config )
        {
            return new SettingsModel(
                AwakeHour: Enums.TryConvertEnum<Hour>( config.AwakeTime?.Hour ?? 0 ) ?? Hour.Hour_0,
                AwakeMinute: Enums.TryConvertEnum<Minute>( config.AwakeTime?.Minute ?? 0 ) ?? Minute.Minute_0,
                DisableAwake: config.AwakeTime is null,
                SleepHour: Enums.TryConvertEnum<Hour>( config.SleepTime?.Hour ?? 0 ) ?? Hour.Hour_0,
                SleepMinute: Enums.TryConvertEnum<Minute>( config.SleepTime?.Minute ?? 0 ) ?? Minute.Minute_0,
                DisableSleep: config.SleepTime is null,
                Brightness: Enums.TryConvertEnum<Brightness>( config.Brightness ) ?? Brightness.Brightness_75,
                ChangeInterval: Enums.TryConvertEnum<ChangeIntervals>( (int)config.PhotoChangeInterval.TotalSeconds ) ?? ChangeIntervals.Minutes_1,
                "",
                ""
            );
        }
    }
}
