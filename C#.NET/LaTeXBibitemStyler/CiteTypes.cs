using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaTeXBibitemStyler
{
    public enum CiteTypes
    {
        /// <summary>
        /// no tags found
        /// </summary>
        END = 0,

        /// <summary>
        /// \cite tag
        /// </summary>
        CITE = 1, 
        
        /// <summary>
        /// \citenum tag
        /// </summary>
        CITENUM = 2, 
        
        /// <summary>
        /// \nocite tag
        /// </summary>
        NOCITE = 3
    }
}
