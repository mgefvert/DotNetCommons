using System;
using System.Linq;
using DotNetCommons.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Collections;

[TestClass]
public class CollectionLinkerTest
{
    public class Post
    {
        public int ID { get; set; }
        public Comment[]? Comments { get; set; }
    }

    public class Comment
    {
        public int ID { get; set; }
        public int PostID { get; set; }
        public Post? Post { get; set; }
    }

    private Post[] _posts = null!;
    private Comment[] _comments = null!;

    [TestInitialize]
    public void Setup()
    {
        _posts = new Post[]
        {
            new() { ID = 1 },
            new() { ID = 2 },
            new() { ID = 3 },
        };

        _comments = new Comment[]
        {
            new() { ID = 1000, PostID = 1 },
            new() { ID = 1001, PostID = 1 },
            new() { ID = 1002, PostID = 1 },
            new() { ID = 1003, PostID = 3 },
        };
    }

    [TestMethod]
    public void TestLinkToMany()
    {
        CollectionLinker.LinkToMany(_posts, _comments, x => x.ID, x => x.PostID,
            (post, comments) => post.Comments = comments.ToArray());

        Assert.AreEqual(3, _posts[0].Comments!.Length);
        Assert.AreEqual(0, _posts[1].Comments!.Length);
        Assert.AreEqual(1, _posts[2].Comments!.Length);

        Assert.AreEqual(1000, _posts[0].Comments![0].ID);
        Assert.AreEqual(1001, _posts[0].Comments![1].ID);
        Assert.AreEqual(1002, _posts[0].Comments![2].ID);
        Assert.AreEqual(1003, _posts[2].Comments![0].ID);
    }

    [TestMethod]
    public void TestLinkToOne()
    {
        CollectionLinker.LinkToOne(_comments, _posts, x => x.PostID, x => x.ID, 
            (comment, post) => comment.Post = post);

        Assert.AreEqual(1, _comments[0].Post!.ID);
        Assert.AreEqual(1, _comments[1].Post!.ID);
        Assert.AreEqual(1, _comments[2].Post!.ID);
        Assert.AreEqual(3, _comments[3].Post!.ID);
    }
}