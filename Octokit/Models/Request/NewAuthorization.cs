using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Octokit
{
    /// <summary>
    /// Used to create a new authorization.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class NewAuthorization
    {
        // TODO: I'd love to not need this
        public NewAuthorization()
        {
        }

        public NewAuthorization(string note, string[] scopes)
        {
            Scopes = scopes;
            Note = note;
        }

        public NewAuthorization(string note, string[] scopes, string fingerprint)
        {
            Scopes = scopes;
            Note = note;
            Fingerprint = fingerprint;
        }

        /// <summary>
        /// Replaces the authorization scopes with this list.
        /// </summary>
        public IEnumerable<string> Scopes { get; set; }

        /// <summary>
        /// Optional parameter that allows an OAuth application to create multiple authorizations for a single user
        /// </summary>
        public string Fingerprint { get; set; }

        /// <summary>
        /// An optional note to remind you what the OAuth token is for.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// An optional URL to remind you what app the OAuth token is for.
        /// </summary>
        public string NoteUrl { get; set; }

        internal string DebuggerDisplay
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, "Note: {0}", Note);
            }
        }
    }
}
