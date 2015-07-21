using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaTeXBibitemStyler
{
    public enum BibStyles
    {
        /// <summary>
        /// plain style (what comes in comes out unchanged)
        /// </summary>
        PLAIN, 
        
        /// <summary>
        /// alphabetical sorting
        /// </summary>
        ALPHA, 
        
        /// <summary>
        /// unsorted sorting (order of appearance of the cites)
        /// </summary>
        UNSRT
    }
}
