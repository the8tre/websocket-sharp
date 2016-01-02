/*
 * HttpListenerPrefix.cs
 *
 * This code is derived from System.Net.ListenerPrefix.cs of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2012-2014 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

/*
 * Authors:
 * - Gonzalo Paniagua Javier <gonzalo@novell.com>
 * - Oleg Mihailik <mihailik@gmail.com>
 */

namespace WebSocketSharp.Net
{
    using System;

    internal sealed class HttpListenerPrefix
    {
        string _host;
        HttpListener _listener;

        readonly string _original;
        string _path;
        ushort _port;

        // Must be called after calling HttpListenerPrefix.CheckPrefix.
        public HttpListenerPrefix(string uriPrefix)
        {
            _original = uriPrefix;
            Parse(uriPrefix);
        }

        public string Host => _host;

        public HttpListener Listener
        {
            get
            {
                return _listener;
            }

            set
            {
                _listener = value;
            }
        }

        public string Path => _path;

        public int Port => _port;

        private void Parse(string uriPrefix)
        {
            var defaultPort = uriPrefix.StartsWith("https://") ? 443 : 80;

            var len = uriPrefix.Length;
            var startHost = uriPrefix.IndexOf(':') + 3;
            var colon = uriPrefix.IndexOf(':', startHost, len - startHost);
            var root = 0;
            if (colon > 0)
            {
                root = uriPrefix.IndexOf('/', colon, len - colon);
                _host = uriPrefix.Substring(startHost, colon - startHost);
                _port = (ushort)int.Parse(uriPrefix.Substring(colon + 1, root - colon - 1));
            }
            else
            {
                root = uriPrefix.IndexOf('/', startHost, len - startHost);
                _host = uriPrefix.Substring(startHost, root - startHost);
                _port = (ushort)defaultPort;
            }

            _path = uriPrefix.Substring(root);

            var pathLen = _path.Length;
            if (pathLen > 1)
                _path = _path.Substring(0, pathLen - 1);
        }

        public static void CheckPrefix(string uriPrefix)
        {
            if (uriPrefix == null)
            {
                throw new ArgumentNullException("uriPrefix");
            }

            var len = uriPrefix.Length;
            if (len == 0)
            {
                throw new ArgumentException("An empty string.");
            }

            if (!(uriPrefix.StartsWith("http://") || uriPrefix.StartsWith("https://")))
            {
                throw new ArgumentException("The scheme isn't 'http' or 'https'.");
            }

            var startHost = uriPrefix.IndexOf(':') + 3;
            if (startHost >= len)
            {
                throw new ArgumentException("No host is specified.");
            }

            var colon = uriPrefix.IndexOf(':', startHost, len - startHost);
            if (startHost == colon)
            {
                throw new ArgumentException("No host is specified.");
            }

            if (colon > 0)
            {
                var root = uriPrefix.IndexOf('/', colon, len - colon);
                if (root == -1)
                {
                    throw new ArgumentException("No path is specified.");
                }

                int port;
                if (!int.TryParse(uriPrefix.Substring(colon + 1, root - colon - 1), out port) || !port.IsPortNumber())
                {
                    throw new ArgumentException("An invalid port is specified.");
                }
            }
            else
            {
                var root = uriPrefix.IndexOf('/', startHost, len - startHost);
                if (root == -1)
                {
                    throw new ArgumentException("No path is specified.");
                }
            }

            if (uriPrefix[len - 1] != '/')
            {
                throw new ArgumentException("Ends without '/'.");
            }
        }

        // Equals and GetHashCode are required to detect duplicates in any collection.
        public override bool Equals(object obj)
        {
            var pref = obj as HttpListenerPrefix;
            return pref != null && pref._original == _original;
        }

        public override int GetHashCode()
        {
            return _original.GetHashCode();
        }

        public override string ToString()
        {
            return _original;
        }
    }
}
