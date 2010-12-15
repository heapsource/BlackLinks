using System;
using BlackLinks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace sampleApplication.Controllers
{
	[Controller(Route="/Patients")]
	public class Patients : Controller
	{
		[Action]
		public class Index : BlackAction 
		{
			public override void OnExecute ()
			{
				/*
				this.Context.Request.ResponseContentType = "text/html";
				this.Context.Request.Write("<h1>Patients Controller!!</h1>");
				string connectionString = "mongodb://127.0.0.1";
				MongoServer server = MongoServer.Create (connectionString);
				MongoDatabase test = server.GetDatabase ("test");
				test.GetCollection("Patients").Save(new BsonDocument(new BsonElement("Name",BsonValue.Create("Johan Hernandnez"))));
				this.Context.Request.Write ("<p>Inserted</p>");
				*/			
				this.RenderView ("Index.html");
			}
		}
		public class New : BlackAction
		{
			public override void OnExecute ()
			{
				this.RenderView ("New.html");
			}
		}
		public class Show : BlackAction
		{
			public override void OnExecute ()
			{
				this.RenderView ("Show.html");
			}
		}
	}
}

