using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MiniBlogElasticsearch.Models;
using Nest;

namespace MiniBlogElasticsearch.Controllers
{
    public class SearchController : Controller
    {
        IElasticClient _elasticClient;
        IOptionsSnapshot<BlogSettings> _settings;
        public SearchController(IElasticClient elasticClient, IOptionsSnapshot<BlogSettings> settings)
        {
            _elasticClient = elasticClient;
            _settings = settings;
        }

        [Route("/search")]
        public async Task<IActionResult> Find(string query, int page = 1, int pageSize = 5)
        {
            var response = await _elasticClient.SearchAsync<Post>(
                s => s.Query(q => q.QueryString(d => d.Query(query)))
                    .From((page - 1) * pageSize)
                    .Size(pageSize));

            ViewData["Title"] = _settings.Value.Name + " - Search Results";
            ViewData["Description"] = _settings.Value.Description;

            if (!response.IsValid)
            {
                // We could handle errors here by checking response.OriginalException or response.ServerError properties
                return View("Results", new Post[] { });
            }

            if (page > 1)
                ViewData["prev"] = GetSearchUrl(query, page - 1, pageSize);
            if (response.IsValid && response.Total > page * pageSize)
                ViewData["next"] = GetSearchUrl(query, page + 1, pageSize);

            return View("Results", response.Documents);
        }

        private static string GetSearchUrl(string query, int page, int pageSize)
        {
            return $"/search?query={Uri.EscapeDataString(query ?? "")}&page={page}&pagesize={pageSize}/";
        }
    }
}