using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Tests.Serialization;

public class SzTest
{
    [Test]
    public void TestGameplayModifiers()
    {
        Span<byte> buffer = stackalloc byte[1024];

        // ---
        
        var writeGm = new GameplayModifiers(
            energyType: EnergyType.Battery,
            noFailOn0Energy: true,
            instaFail: false,
            failOnSaberClash: true,
            enabledObstacleType: EnabledObstacleType.NoObstacles,
            fastNotes: true,
            strictAngles: false,
            disappearingArrows: true,
            ghostNotes: false,
            noBombs: true,
            songSpeed: SongSpeed.Slower,
            noArrows: false,
            proMode: true,
            zenMode: false,
            smallCubes: true
        );

        var writer = new NetWriter(buffer);
        writeGm.WriteTo(ref writer);

        // ---

        var reader = new NetReader(buffer);
        var readGm = reader.ReadSerializable<GameplayModifiers>();
        Assert.Multiple(() =>
        {
            Assert.That(readGm.EnergyType, Is.EqualTo(writeGm.EnergyType));
            Assert.That(readGm.NoFailOn0Energy, Is.EqualTo(writeGm.NoFailOn0Energy));
            Assert.That(readGm.InstaFail, Is.EqualTo(writeGm.InstaFail));
            Assert.That(readGm.FailOnSaberClash, Is.EqualTo(writeGm.FailOnSaberClash));
            Assert.That(readGm.EnabledObstacleType, Is.EqualTo(writeGm.EnabledObstacleType));
            Assert.That(readGm.FastNotes, Is.EqualTo(writeGm.FastNotes));
            Assert.That(readGm.StrictAngles, Is.EqualTo(writeGm.StrictAngles));
            Assert.That(readGm.DisappearingArrows, Is.EqualTo(writeGm.DisappearingArrows));
            Assert.That(readGm.GhostNotes, Is.EqualTo(writeGm.GhostNotes));
            Assert.That(readGm.NoBombs, Is.EqualTo(writeGm.NoBombs));
            Assert.That(readGm.SongSpeed, Is.EqualTo(writeGm.SongSpeed));
            Assert.That(readGm.NoArrows, Is.EqualTo(writeGm.NoArrows));
            Assert.That(readGm.ProMode, Is.EqualTo(writeGm.ProMode));
            Assert.That(readGm.ZenMode, Is.EqualTo(writeGm.ZenMode));
            Assert.That(readGm.SmallCubes, Is.EqualTo(writeGm.SmallCubes));
        });
        Assert.That(reader.Position, Is.EqualTo(writer.Position), "Reader position should be at the end of buffer contents");
    }
}