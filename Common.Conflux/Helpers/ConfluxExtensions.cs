using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;

namespace Conflux.Helpers
{
    public static class ConfluxExtensions
    {
        public static bool IsEmpty(this String source)
        {

            return String.IsNullOrEmpty(source);

        }
    }
}
