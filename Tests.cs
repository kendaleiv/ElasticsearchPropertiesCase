using Elasticsearch.Net;
using Nest;
using System.Threading.Tasks;
using Xunit;

namespace ElasticsearchPropertiesCase
{
    public class Tests
    {
        private const string TestIndex = "test-index";

        [Fact]
        public async Task Defaults()
        {
            var elasticSearch = new ElasticsearchInside.Elasticsearch();
            await elasticSearch.Ready();

            var elasticClient = new ElasticClient(
                new ConnectionSettings(elasticSearch.Url)
                    // https://github.com/elastic/elasticsearch-net/issues/1528#issuecomment-134221775
                    // .DefaultFieldNameInferrer(x => x)
            );

            var createIndexResponse = await elasticClient.CreateIndexAsync(TestIndex, i => i
                .Mappings(m => m
                    .Map<Person>(map => map
                        .AutoMap()
                        .Properties(p => p.Text(t => t.Name(n => n.GivenName))))));

            var indexResponse = await elasticClient.IndexAsync(new Person
            {
                GivenName = "Test",
                FamilyName = "Person"
            }, i => i.Index(TestIndex).Refresh(Refresh.WaitFor));

            var results = await elasticClient.SearchAsync<Person>(s => s
                .Index(TestIndex)
                .Query(
                    y => y.MultiMatch(
                        mm => mm.Query("test")
                                // .Fields(f => f.Field(nameof(Person.GivenName))))));
                                .Fields(f => f.Field("givenName")))));

            var results2 = await elasticClient.SearchAsync<Person>(s => s
                .Index(TestIndex));

            Assert.Single(results.Documents);
        }

        [Fact]
        public async Task SpecifyDefaultFieldNameInferrerEmpty()
        {
            var elasticSearch = new ElasticsearchInside.Elasticsearch();
            await elasticSearch.Ready();

            var elasticClient = new ElasticClient(
                new ConnectionSettings(elasticSearch.Url)
                    // https://github.com/elastic/elasticsearch-net/issues/1528#issuecomment-134221775
                    .DefaultFieldNameInferrer(x => x)
            );

            var createIndexResponse = await elasticClient.CreateIndexAsync(TestIndex, i => i
                .Mappings(m => m
                    .Map<Person>(map => map
                        .AutoMap()
                        .Properties(p => p.Text(t => t.Name(n => n.GivenName))))));

            var indexResponse = await elasticClient.IndexAsync(new Person
            {
                GivenName = "Test",
                FamilyName = "Person"
            }, i => i.Index(TestIndex).Refresh(Refresh.WaitFor));

            var results = await elasticClient.SearchAsync<Person>(s => s
                .Index(TestIndex)
                .Query(
                    y => y.MultiMatch(
                        mm => mm.Query("test")
                                .Fields(f => f.Field(nameof(Person.GivenName))))));
                                // .Fields(f => f.Field("givenName")))));

            var results2 = await elasticClient.SearchAsync<Person>(s => s
                .Index(TestIndex));

            Assert.Single(results.Documents);
        }
    }
}
