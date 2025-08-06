using DotNetCommons.Numerics;
using FluentAssertions;

namespace DotNetCommonTests.Numerics;

[TestClass]
public class Federated48KeyTests
{
    [TestMethod]
    public void MakeFederatedKey_SystemKeyTooLarge() => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Federated48Key.MakeFederatedKey(0x8000, 1));

    [TestMethod]
    public void MakeFederatedKey_SystemKeyZero() => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Federated48Key.MakeFederatedKey(0, 1));

    [TestMethod]
    public void MakeFederatedKey_RecordKeyTooLarge() => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Federated48Key.MakeFederatedKey(1, 0x1_0000_0000_0000));

    [TestMethod]
    public void MakeFederatedKey_RecordKeyZero() => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Federated48Key.MakeFederatedKey(1, 0));

    [TestMethod]
    public void MakeFederatedKey_Works()
    {
        Federated48Key.MakeFederatedKey(1, 1).Should().Be(0x0002_0000_0000_0002);
        Federated48Key.MakeFederatedKey(42, 123456).Should().Be(0x0054_0000_0003_C481);
        Federated48Key.MakeFederatedKey(1000, 9876543210).Should().Be(0x07D0_0004_9960_2DD5);
        Federated48Key.MakeFederatedKey(32767, 42).Should().Be(0xFFFE_0000_0000_0054);
        Federated48Key.MakeFederatedKey(0x7FFF, 0xFFFF_FFFF_FFFF).Should().Be(0xFFFF_FFFF_FFFF_FFFF);
    }

    [TestMethod]
    public void SplitFederatedKey_Works()
    {
        Federated48Key.SplitFederatedKey(0x0002_0000_0000_0002).Should().Be((1, 1, true));
        Federated48Key.SplitFederatedKey(0x0054_0000_0003_C481).Should().Be((42, 123456, true));
        Federated48Key.SplitFederatedKey(0x07D0_0004_9960_2DD5).Should().Be((1000, 9876543210, true));
        Federated48Key.SplitFederatedKey(0xFFFE_0000_0000_0054).Should().Be((32767, 42, true));
        Federated48Key.SplitFederatedKey(0xFFFF_FFFF_FFFF_FFFF).Should().Be((0x7FFF, 0xFFFF_FFFF_FFFF, true));

        Federated48Key.SplitFederatedKey(0x0002_0000_0000_0003).Should().Be((1, 1, false));
        Federated48Key.SplitFederatedKey(0x0000_0000_0003_C481).Should().Be((0, 123456, false));
        Federated48Key.SplitFederatedKey(0x0054_0000_0000_0001).Should().Be((42, 0, false));
        Federated48Key.SplitFederatedKey(0xFFFF_FFFF_FFFF_FFFE).Should().Be((0x7FFF, 0xFFFF_FFFF_FFFF, false));
    }
}
