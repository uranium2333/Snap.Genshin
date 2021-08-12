﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Snap.Framework.Attributes
{
    /// <summary>
    /// 指示该类或方法来自 Github 上的项目
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class GithubAttribute : Attribute
    {
        public GithubAttribute(string repoUrl)
        {

        }
    }
}