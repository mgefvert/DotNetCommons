using DotNetCommons.Collections;
using FluentAssertions;

namespace DotNetCommonTests.Collections;

[TestClass]
public class GridTest
{
    public class Names
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Names(int id, string firstName, string lastName) =>
            (Id, FirstName, LastName) = (id, firstName, lastName);
    }

    [TestMethod]
    public void BuildFromObjects_Works()
    {
        var objects = new[]
        {
            new Names(1, "Alex", "Ableton"),
            new Names(2, "Bella", "Barley"),
            new Names(3, "Charlie", "Chaplin")
        };

        var csv = Grid.BuildFromObjects(objects, n => n.Id)
            .ToCsv(new GridRenderOptions<int, string, string?>
            {
                IncludeColumnHeader = true
            });
        csv.Should().NotBeEmpty();

        csv = csv!.Replace('\"', '\''); // For easier debugging
        csv.Should().Be("'Id','FirstName','LastName'" + Environment.NewLine +
                        "1,'Alex','Ableton'" + Environment.NewLine +
                        "2,'Bella','Barley'" + Environment.NewLine +
                        "3,'Charlie','Chaplin'" + Environment.NewLine);
    }
}