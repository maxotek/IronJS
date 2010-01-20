﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronJS.Tests
{
    [TestClass]
    public class ObjectTests
    {
        [TestMethod]
        public void TestObjectCreateNew()
        {
            Assert.AreEqual(
                "[object Object]",
                ScriptRunner.Run(
                    "emit(new Object())"
                )
            );
        }

        [TestMethod]
        public void TestObjectCreateNewShorthand()
        {
            Assert.AreEqual(
                "[object Object]",
                ScriptRunner.Run(
                    "emit({})"
                )
            );
        }

        [TestMethod]
        public void TestObjectAssignProperty()
        {
            Assert.AreEqual(
                "hello world",
                ScriptRunner.Run(
                    "foo = {};"
                    + "foo.bar = 'hello world';"
                    + "emit(foo.bar);"
                )
            );
        }
    }
}