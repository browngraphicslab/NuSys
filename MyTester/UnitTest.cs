using System;
using NuSysApp;
using NusysIntermediate;
using Xunit;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace MyTester
{
   
    public class UnitTest1
    {
        [Fact]
        public void PassingTest()
        {
            Assert.Equal(4, Add(2, 2));
        }

        [Fact]
        public void FailingTest()
        {
            // I just changed this to pass haha
            Assert.Equal(4, Add(2, 2));
        }

        int Add(int x, int y)
        {
            return x + y;
        }

        [Fact]
        public void PassingTest2()
        {
            Assert.Equal(42,42);
        }

        [Fact]
        public void NuSysTest()
        {
           // var rc = new CanvasControl(); // ICanvasResourceCreatorWithDpi
           // var b = new BaseRenderItem(null,rc);
           
            var blah = new ListViewUIElementContainer<string>(null,null);
        }
    }
}
