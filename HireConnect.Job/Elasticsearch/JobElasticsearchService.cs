using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using HireConnect.Job.Middleware;

namespace HireConnect.Job.Elasticsearch
{
    /// <summary>
    /// Implementation of IJobElasticsearchService using Elastic.Clients.Elasticsearch v8.
    /// Uses object-initializer style (NOT lambda descriptors) — the correct API for v8.x.
    /// Index: "jobs"
    /// </summary>
    public class JobElasticsearchService : IJobElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly ILogger<JobElasticsearchService> _logger;
        private const string IndexName = "jobs";

        public JobElasticsearchService(ElasticsearchClient client,
                                       ILogger<JobElasticsearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        // ── Index Management ────────────────────────────────────────────────────

        public async Task EnsureIndexAsync()
        {
            var existsResponse = await _client.Indices.ExistsAsync(IndexName);
            if (existsResponse.Exists)
                return;

            // Use typed descriptor against JobDocument for field-name inference
            var createResponse = await _client.Indices.CreateAsync<JobDocument>(IndexName, c => c
                .Mappings(m => m
                    .Properties(p => p
                        .IntegerNumber(f => f.JobId)
                        .Text(f => f.Title)
                        .Keyword(f => f.Category)
                        .Keyword(f => f.Type)
                        .Text(f => f.Location)
                        .DoubleNumber(f => f.SalaryMin)
                        .DoubleNumber(f => f.SalaryMax)
                        .Text(f => f.Description)
                        .Keyword(f => f.Skills)
                        .IntegerNumber(f => f.ExperienceRequired)
                        .IntegerNumber(f => f.PostedBy)
                        .Keyword(f => f.Status)
                        .Date(f => f.PostedAt)
                    )
                )
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(1)
                )
            );

            if (!createResponse.IsValidResponse)
            {
                _logger.LogError("Failed to create ES index '{Index}': {Error}",
                    IndexName, createResponse.DebugInformation);
                throw new ElasticsearchException($"Failed to create index '{IndexName}'.");
            }

            _logger.LogInformation("Elasticsearch index '{Index}' created.", IndexName);
        }

        // ── Document Sync ───────────────────────────────────────────────────────

        public async Task IndexJobAsync(JobDocument document)
        {
            var request = new IndexRequest<JobDocument>(document, IndexName, document.JobId.ToString());
            var response = await _client.IndexAsync(request);

            if (!response.IsValidResponse)
            {
                _logger.LogError("Failed to index job {JobId}: {Error}",
                    document.JobId, response.DebugInformation);
                throw new ElasticsearchException($"Failed to index job {document.JobId}.");
            }

            _logger.LogInformation("Job {JobId} indexed in Elasticsearch.", document.JobId);
        }

        public async Task DeleteJobAsync(int jobId)
        {
            var response = await _client.DeleteAsync(IndexName, jobId.ToString());

            if (!response.IsValidResponse && response.Result != Result.NotFound)
            {
                _logger.LogError("Failed to delete job {JobId} from ES: {Error}",
                    jobId, response.DebugInformation);
                throw new ElasticsearchException($"Failed to delete job {jobId} from Elasticsearch.");
            }

            _logger.LogInformation("Job {JobId} removed from Elasticsearch.", jobId);
        }

        // ── Search ──────────────────────────────────────────────────────────────

        public async Task<IEnumerable<JobDocument>> SearchAsync(
            string keyword,
            double? minSalary = null,
            double? maxSalary = null,
            string? category = null,
            string? location = null,
            int? experienceRequired = null,
            int from = 0,
            int size = 20)
        {
            // ── Build must clauses ──────────────────────────────────────────────
            var mustClauses = new List<Query>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // Full-text search across title, description, skills
                mustClauses.Add(new MultiMatchQuery
                {
                    Fields = new[] { "title", "description", "skills" },
                    Query = keyword,
                    Type = TextQueryType.BestFields,
                    Fuzziness = new Fuzziness("AUTO")
                });
            }
            else
            {
                mustClauses.Add(new MatchAllQuery());
            }

            // ── Build filter clauses ────────────────────────────────────────────
            var filterClauses = new List<Query>
            {
                // Always restrict to Active jobs
                new TermQuery("status") { Value = "Active" }
            };

            if (!string.IsNullOrWhiteSpace(category))
            {
                filterClauses.Add(new TermQuery("category") { Value = category });
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                filterClauses.Add(new MatchQuery("location") { Query = location });
            }

            if (minSalary.HasValue || maxSalary.HasValue)
            {
                var salaryRange = new NumberRangeQuery("salaryMin");
                if (minSalary.HasValue) salaryRange.Gte = minSalary.Value;
                if (maxSalary.HasValue) salaryRange.Lte = maxSalary.Value;
                filterClauses.Add(salaryRange);
            }

            if (experienceRequired.HasValue)
            {
                filterClauses.Add(new NumberRangeQuery("experienceRequired")
                {
                    Lte = experienceRequired.Value
                });
            }

            // ── Execute ─────────────────────────────────────────────────────────
            var searchRequest = new SearchRequest(IndexName)
            {
                From = from,
                Size = size,
                Query = new BoolQuery
                {
                    Must = mustClauses,
                    Filter = filterClauses
                },
                Sort = new List<SortOptions>
                {
                    SortOptions.Field("postedAt", new FieldSort { Order = SortOrder.Desc })
                }
            };

            var response = await _client.SearchAsync<JobDocument>(searchRequest);

            if (!response.IsValidResponse)
            {
                _logger.LogError("Elasticsearch search failed: {Error}", response.DebugInformation);
                throw new ElasticsearchException("Search query failed. Please try again.");
            }

            return response.Documents ?? Enumerable.Empty<JobDocument>();
        }
    }
}