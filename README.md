<br />
<p align="center">
  <h3 align="center">ElasticSearch Library</h3>
</p>

# Table of Contents

- [Table of Contents](#table-of-contents)
- [About The Project](#about-the-project)
- [Getting started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Setup](#setup)
  - [Check if ElasticSearch is running](#check-if-elasticsearch-is-running)
- [How do I use this library in my Project?](#how-do-i-use-this-library-in-my-project-)
  - [Include the project in your solution](#include-the-project-in-your-solution)
  - [As a nuget package](#as-a-nuget-package)
- [Configuration](#configurations)
  - [Config file](#config-file)
  - [Initialize Elasticsearch](#initialize-elasticsearch)
- [How to index data?](#how-to-index-data-)
  - [Add entity](#add-entity)
  - [Add bulk entities](#add-bulk-entities)
  - [Update entity](#update-entity)
  - [Update bulk entities](#update-bulk-entities)
  - [delete entity](#delete-entity)  
  - [Delete bulk entities](#delete-bulk-entities)
  - [Recreate index](#recreate-index)
- [How to perform search?](#how-to-perform-search-)
  
# About The Project
This is a library for working with ElasticSearch. It is providing the basic functionality with ElasicSearch. The library provide the folowing functionalities:
- Create/Re-create index
- Get a record from index by id
- Add/Update/Delete record from index
- Bulk Add/Update/Delete recods from index
- Search functionality that accept a dynamic query as parameter with pagination, sorting and aggregations
 
 If you have not used Elasticsearch before you can check this presentation for the basic concepts and how Elastic search works: [Elasticsearch for beginners](docs/Elastic-Search-Presentation.pdf)
 
# Getting started

## Prerequisites

This project requires [Elasic Search](https://www.elastic.co) that can be downloaded from [here](https://www.elastic.co/downloads/elasticsearch)

## Setup

- Download and unzip Elastic Search

- Run bin/elasticsearch (or bin\elasticsearch.bat on Windows)


## Check if ElasticSearch is running

- Run curl http://localhost:9200/, Invoke-RestMethod http://localhost:9200 with PowerShell or go to [http://localhost:9200/](http://localhost:9200/) on browser

Response:
```json
{
  "name" : "ElasticSearch-TestServer",
  "cluster_name" : "elasticsearch",
  "cluster_uuid" : "HXeACs45RzSybQ4PiLYeEA",
  "version" : {
    "number" : "7.11.2",
    "build_flavor" : "default",
    "build_type" : "zip",
    "build_hash" : "3e5a16cfec50876d20ea77b075070932c6464c7d",
    "build_date" : "2021-03-06T05:54:38.141101Z",
    "build_snapshot" : false,
    "lucene_version" : "8.7.0",
    "minimum_wire_compatibility_version" : "6.8.0",
    "minimum_index_compatibility_version" : "6.0.0-beta1"
  },
  "tagline" : "You Know, for Search"
}
```

# How do I use this library in my Project?

There are two ways to use this library in your project:
- Include the project in your solution
- As a nuget package(not available yet).  

## Include the project in your solution

Download the project from git and inclode the project in your VS solution.

## As a nuget package
 - A nuget package will be provided soon.
 
# Configuration

## Config file

Elasicsearch configuration in project .config file:

```json
"ElasticSearch": {
        "Connection": "http://localhost:9200",
        "NumberOfReplicas": 5,
        "NumberOfShards": 5
    }
```

## Initialize Elasticsearch

In .NET core and .NET 5 Elasic Search can be initialized using depency injection.
Call the following method in Startup.cs

```c#
private static void InitializeElasticSearch(IServiceCollection services, IConfiguration configuration, IHostingEnvironment environment = null)
{
	var connectionString = $"{configuration["ElasticSearch:Connection"]}";
	var numberOfReplicasConfig = $"{configuration["ElasticSearch:NumberOfReplicas"]}";
	var numberOfShardsConfig = $"{configuration["ElasticSearch:NumberOfShards"]}";

	if (string.IsNullOrWhiteSpace(numberOfReplicasConfig) || int.TryParse(numberOfReplicasConfig, out var numberOfReplicas) == false)
	{
		numberOfReplicas = 5;
	}

	if (string.IsNullOrWhiteSpace(numberOfShardsConfig) || int.TryParse(numberOfShardsConfig, out var numberOfShards))
	{
		numberOfShards = 5;
	}

		var enableDebugMode = environment != null && environment.IsDevelopment();

	services.AddSingleton(new ElasticSearchSettings(numberOfReplicas, numberOfShards));
	services.AddSingleton<IElasticClient>(ElasticSearchSetup.Initialize(connectionString, enableDebugMode));
	services.AddSingleton<ISearchService, SearchService>();
}
```

# How to index data?

When you add a new entity the index will be created automaticaly based on entity name and the current culture. If the index is already created the entity will be added into that index.

The object that is send to be indexed in Elastic search requires to have an **Id** property defined. The value from **Id** property is used to define the record **_id** in Elastic search. The Type for the **Id** property can be Guid, string, long, integer.
 

## Add entity

Adding an entity into Elastic search can be made using the method: 
```c#
Task<Response> AddEntityAsync<TEntity>(TEntity entity)where TEntity : class
```
where TEntity it an object. It can be the entity that is loaded from a database, a DTO or a custom object. 

## Add bulk entities

Adding a collection of entities into Elastic search can be made using the method: 
```c#
Task<Response> AddEntitiesAsync<TEntity>(IList<TEntity> entities) where TEntity : class;
```
where TEntity it a collection of objects of the same type.

## Update entity

Update an entity into Elastic search can be made using the method: 
```c#
Task<Response> UpdateEntityAsync<TEntity>(TEntity entity)where TEntity : class;
```

where TEntity it an object. 

## Add bulk entities

Update acollection of entities into Elastic search can be made using the method: 
```c#
Task<BulkResponse> AddOrUpdateEntitiesAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
```

where TEntity it a collection of object of the same type.
This method also create the index if not exists and andd a record if not exists

## Delete entity

Delete an entity into Elastic search can be made using one of the following methods: 
```c#
Task<Response> DeleteEntityAsync<TEntity>(TEntity entity) where TEntity : class;
Task<Response> DeleteEntityAsync<TEntity>(Id id) where TEntity : class;
```

The first method accept the entity ans parameter;
The second method delete the record by **Id** where **Id** is of type Id defined in Elastic search NEST package.

## Delete bulk entities

Delete a collection of entities into Elastic search can be made using the method: 
```c#
Task<BulkResponse> DeleteEntitiesAsync<TEntity>(IEnumerable<TEntity> entities)where TEntity : class;
```

where TEntity it a collection of objects of the same type.


## Recreate index

The index can be recreated with this method:
```
Task<Response> RecreateIndexAsync<T>() where T : class;
```

An index need to be recreated in following situations:
- Add a new property to the index.
- Change te index mapping.
- Add/remove/update analyzers defined for an index.
- change the number of shards/replicas. 


# How to perform search?