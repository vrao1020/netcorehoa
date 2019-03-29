using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Sieve.Models;
using System;
using System.Collections.Generic;

namespace HoaWebAPI.Extensions.APIConventions
{
    public static class HoaWebApiConvention
    {
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public static void Create([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] object model) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public static void Create([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid Id,
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] object model) { }


        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public static void Create([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] IEnumerable<object> models) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public static void Delete([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public static void Delete([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid Id,
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(404)]
        public static void Block([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(404)]
        public static void Block([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid Id,
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id) { }


        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public static void Get([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public static void Get([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid Id,
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id) { }


        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(200)]
        public static void Get([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] SieveModel sieveModel) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public static void Get([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid Id,
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] SieveModel sieveModel) { }


        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public static void Collection([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] IEnumerable<Guid> ids) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public static void GetCurrent() { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public static void Activate() { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public static void Deactivate() { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public static void Partial([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id, 
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] object model) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public static void Partial([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid Id,
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id, 
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] object model) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public static void Partial([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] object model) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public static void Update([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id, 
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] object model) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public static void Update([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid Id,
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id, 
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] object model) { }

        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public static void Update([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] object model) { }


    }
}
