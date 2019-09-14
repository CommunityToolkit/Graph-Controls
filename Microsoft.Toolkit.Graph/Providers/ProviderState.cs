namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// <see cref="ProviderState"/> represents the current authentication state of the session for a given <see cref="IProvider"/>.
    /// </summary>
    public enum ProviderState
    {
        /// <summary>
        /// The user's status is not known.
        /// </summary>
        Loading,

        /// <summary>
        /// The user is signed-out.
        /// </summary>
        SignedOut,

        /// <summary>
        /// The user is signed-in.
        /// </summary>
        SignedIn
    }
}
