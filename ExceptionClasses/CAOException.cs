using Bumbo.ViewModels;

using Bumbo.ViewModels;
using System;

namespace Bumbo.ExceptionClasses
{
    public class CAOException : Exception
    {
        public CAOErrorViewModel CAOError { get; }

        public CAOException(CAOErrorViewModel caoError)
        {
            CAOError = caoError ?? throw new ArgumentNullException(nameof(caoError));
        }
    }
}