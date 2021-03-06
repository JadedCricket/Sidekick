using System;
using MediatR;

namespace Sidekick.Domain.App.Commands
{
    /// <summary>
    /// Opens the specified Uri in a web browser
    /// </summary>
    public class OpenBrowserCommand : ICommand
    {
        /// <summary>
        /// Opens the specified Uri in a web browser
        /// </summary>
        /// <param name="uri">The uri to open in a browser</param>
        public OpenBrowserCommand(Uri uri)
        {
            Uri = uri;
        }

        /// <summary>
        /// The uri to open in a browser
        /// </summary>
        public Uri Uri { get; }
    }
}
