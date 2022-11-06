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

using System.Text;
using Serilog.Core;
using Serilog.Events;
using SethCS.Extensions;

namespace PiPictureFrame.Web
{
    public sealed class InProcessLogSink : ILogEventSink
    {
        // ---------------- Fields ----------------

        private const int maxSize = 512;

        private readonly Queue<string> messages;

        private readonly IFormatProvider? formatProvider;

        // ---------------- Constructor ----------------

        public InProcessLogSink( IFormatProvider? formatProvider )
        {
            this.formatProvider = formatProvider;
            this.messages = new Queue<string>( maxSize );
        }

        // ---------------- Functions ----------------

        void ILogEventSink.Emit( LogEvent logEvent )
        {
            string message = $"{DateTime.Now.ToTimeStampString()}> [{logEvent.Level}] {logEvent.RenderMessage( this.formatProvider )}";
            lock( this.messages )
            {
                if( messages.Count >= maxSize )
                {
                    messages.Dequeue();
                }
                messages.Enqueue( message );
            }
        }

        public List<string> ToList()
        {
            lock( this.messages )
            {
                return this.messages.ToList();
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            lock( this.messages )
            {
                foreach( string message in this.messages )
                {
                    builder.AppendLine( message );
                }
            }

            return builder.ToString();
        }
    }
}
