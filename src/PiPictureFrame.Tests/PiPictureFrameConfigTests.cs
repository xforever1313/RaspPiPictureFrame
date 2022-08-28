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

using System.Xml.Linq;

namespace PiPictureFrame.Tests
{
    [TestClass]
    public sealed class PiPictureFrameConfigTests
    {
        // ---------------- Tests ----------------

        [TestMethod]
        public void RoundTripTestWithNullTimesTest()
        {
            // Setup
            var expected = new PiPictureFrameConfig
            {
                AwakeTime = null,
                Brightness = 50,
                PhotoChangeInterval = TimeSpan.Zero,
                PhotoRefreshInterval = TimeSpan.Zero,
                SleepTime = null
            };

            // Act
            XDocument doc = expected.ToXml();
            PiPictureFrameConfig actual = PiPictureFrameConfigExtensions.FromXml( doc );

            // Check
            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void RoundTripTestWithNonNullTimesTest()
        {
            // Setup
            var expected = new PiPictureFrameConfig
            {
                AwakeTime = new TimeOnly( 5, 10, 50 ),
                Brightness = 50,
                PhotoChangeInterval = TimeSpan.FromHours( 1 ),
                PhotoRefreshInterval = TimeSpan.FromHours( 2 ),
                SleepTime = new TimeOnly( 15, 5, 6 )
            };

            // Act
            XDocument doc = expected.ToXml();
            PiPictureFrameConfig actual = PiPictureFrameConfigExtensions.FromXml( doc );

            // Check
            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void RoundTripTestFixUpTest()
        {
            // Setup
            var starting = new PiPictureFrameConfig
            {
                AwakeTime = new TimeOnly( 1, 2, 3 ),
                Brightness = 101,
                PhotoChangeInterval = TimeSpan.FromMilliseconds( -1 ),
                PhotoRefreshInterval = TimeSpan.FromMilliseconds( -1 ),
                SleepTime = new TimeOnly( 11, 12, 13 )
            };
            
            // These values should be fixed-up.
            var expected = starting with
            {
                // 100 is maximum.
                Brightness = 100,

                // Zero is minimum for these.
                PhotoChangeInterval = TimeSpan.Zero,
                PhotoRefreshInterval = TimeSpan.Zero,
            };

            // Act
            XDocument doc = starting.ToXml();
            PiPictureFrameConfig actual = PiPictureFrameConfigExtensions.FromXml( doc );

            // Check
            Assert.AreEqual( expected, actual );
        }
    }
}