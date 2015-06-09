﻿using System;

namespace Octokit.Reactive
{
    /// <summary>
    /// A client for GitHub's User Keys API.
    /// </summary>
    /// <remarks>
    /// See the <a href="http://developer.github.com/v3/users/keys/">User Keys API documentation</a> for more information.
    /// </remarks>
    public interface IObservableUserKeysClient
    {
        /// <summary>
        /// Gets all public keys for the authenticated user.
        /// </summary>
        /// <remarks>
        /// https://developer.github.com/v3/users/keys/#list-your-public-keys
        /// </remarks>
        /// <returns>The <see cref="PublicKey"/>s for the authenticated user.</returns>
        IObservable<PublicKey> GetAll();

        /// <summary>
        /// Gets all verified public keys for a user.
        /// </summary>
        /// <remarks>
        /// https://developer.github.com/v3/users/keys/#list-public-keys-for-a-user
        /// </remarks>
        /// <returns>The <see cref="PublicKey"/>s for the user.</returns>
        IObservable<PublicKey> GetAll(string userName);
    }
}
