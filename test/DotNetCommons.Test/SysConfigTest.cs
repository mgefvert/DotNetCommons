using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test
{
    public class ConfigSampleSection
    {
        public string Key { get; set; }
        public Uri Uri { get; set; }
        public bool Use { get;set; }
    }

    public class ConfigSample
    {
        public string Text { get; set; }
        public ConfigSampleSection Section1 { get; } = new ConfigSampleSection();
        public ConfigSampleSection Section2 { get; } = new ConfigSampleSection();
    }

    public static class ConfigStaticSample
    {
        public static string Text { get; set; }
        public static ConfigSampleSection Section1 { get; } = new ConfigSampleSection();
        public static ConfigSampleSection Section2 { get; } = new ConfigSampleSection();
    }


    [TestClass]
    public class SysConfigTest
    {
        [TestMethod]
        public void Load()
        {
            var cfg = SysConfig.Create<ConfigSample>("sysconfig.xml");

            Assert.AreEqual("Hello!", cfg.Text);
            Assert.AreEqual("abc123", cfg.Section1.Key);
            Assert.AreEqual("http://localhost/", cfg.Section1.Uri.ToString());
            Assert.AreEqual(true, cfg.Section1.Use);
            Assert.AreEqual("abc987", cfg.Section2.Key);
            Assert.AreEqual("http://localhost/", cfg.Section2.Uri.ToString());
            Assert.AreEqual(false, cfg.Section2.Use);
        }

        [TestMethod]
        public void LoadIntoObject()
        {
            var cfg = new ConfigSample();
            SysConfig.LoadIntoObject("sysconfig.xml", cfg);

            Assert.AreEqual("Hello!", cfg.Text);
            Assert.AreEqual("abc123", cfg.Section1.Key);
            Assert.AreEqual("http://localhost/", cfg.Section1.Uri.ToString());
            Assert.AreEqual(true, cfg.Section1.Use);
            Assert.AreEqual("abc987", cfg.Section2.Key);
            Assert.AreEqual("http://localhost/", cfg.Section2.Uri.ToString());
            Assert.AreEqual(false, cfg.Section2.Use);
        }
        
        [TestMethod]
        public void LoadIntoStatic()
        {
            SysConfig.LoadIntoClass("sysconfig.xml", typeof(ConfigStaticSample));

            Assert.AreEqual("Hello!", ConfigStaticSample.Text);
            Assert.AreEqual("abc123", ConfigStaticSample.Section1.Key);
            Assert.AreEqual("http://localhost/", ConfigStaticSample.Section1.Uri.ToString());
            Assert.AreEqual(true, ConfigStaticSample.Section1.Use);
            Assert.AreEqual("abc987", ConfigStaticSample.Section2.Key);
            Assert.AreEqual("http://localhost/", ConfigStaticSample.Section2.Uri.ToString());
            Assert.AreEqual(false, ConfigStaticSample.Section2.Use);
        }
    }
}
