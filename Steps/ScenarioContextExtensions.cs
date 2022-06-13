using MongoDB.Entities;
using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps;

internal static class ScenarioContextExtensions
{
    internal static async Task<T> GetRecord<T>(this ScenarioContext context, string contextKey, Func<T> substitute = null) 
        where T : Entity
        
    {
        if (!context.ContainsKey(contextKey))
        {
            throw new InvalidOperationException($"ScenarioContext does not contain key [{contextKey}]");
        }

        var id = context.Get<string>(contextKey).ToObjectId();
        var record = await DB.Find<T>().OneAsync(id).ConfigureAwait(false);
        if (null == record)
        {
            if (substitute == null)
            {
                throw new InvalidOperationException($"{typeof(T).Name} [{id}] was not found");
            }

            record = substitute.Invoke();
        }
            

        return record;
    }

    internal static async Task<T> GetRecordById<T>(this ScenarioContext context, string id) 
        where T : Entity
        
    {
        var record = await DB.Find<T>().OneAsync(id).ConfigureAwait(false);
        if (null == record)
            throw new InvalidOperationException($"{typeof(T).Name} [{id}] was not found");
        return record;
    }
}