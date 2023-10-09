# RestoDB
RestoDB (Rest over database) is a micro-service that exposes any existing database through REST.
Each table or vue will have its own endpoints to be queried in json or csv. 
You can expose all tables and vues, or select which ones you want to be exposed.
It uses aspnet minimal API for routes declarations and can provide authentication, caching, CORS, Swagger and rate limiting. 

## Providers
Thanks to [SQLKata](https://github.com/sqlkata/querybuilder), RestoDB natively supports SQL Server, PostgreSQL, Mysql and SQLite.

## Docker
Open a terminal and run the following command:
<pre>
docker run --rm --env=DbProvider=<b>&lt;&lt;YOUR_DB_PROVIDER&gt;&gt;</b> --env=DbConnection=<b>"&lt;&lt;YOUR_CONNECTION_STRING&gt;&gt;"</b> -p 3000:80 roddone/restodb
# replace YOUR_DB_PROVIDER and YOUR_CONNECTION_STRING, see Config section for possible values
</pre>

RestoDb is now available on your host at http://localhost:3000.

If you need to use array parameters, you must give and index to the environment variable name, exemple with 'LimitTo' parameter: 
<pre>
docker run --rm --env=DbProvider=&lt;&lt;YOUR_DB_PROVIDER&gt;&gt; --env=DbConnection="&lt;&lt;YOUR_CONNECTION_STRING&gt;&gt;" --env=<b>LimitTo:0=Users</b> --env=<b>LimitTo:1=Cities</b> -p 3000:80 roddone/restodb
# replace YOUR_DB_PROVIDER and YOUR_CONNECTION_STRING, see 'Config' section for possible values
</pre>

If you need to use any of the objects parameters (Config, Authentication, Cors, RateLimiting or Csv), you must prefix by the section's name, exemple with 'Csv' section : 
<pre>
docker run --rm --env=DbProvider=&lt;&lt;YOUR_DB_PROVIDER&gt;&gt; --env=DbConnection="&lt;&lt;YOUR_CONNECTION_STRING&gt;&gt;" --env=<b>Csv:Enabled=true</b> --env=<b>Csv:Separator="|"</b> -p 3000:80 roddone/restodb
# replace YOUR_DB_PROVIDER and YOUR_CONNECTION_STRING, see 'Config' section for possible values
</pre>

## Config
- `DbProvider (string, MANDATORY)` : the database provider that will be used. Possible values are _'NpgSql'_, _'SqlServer'_, _'MySql'_ or _'Sqlite'_
- `DbConnection (string, MANDATORY)`: the database connection string
- `LimitTo (string[])` : entities allowed to expose. **if null or empty, all entities in the database will be exposed**
- `EnableSwagger (boolean)` : indicates if the swagger should be enabled or not
- `ApiSegment (string)` : the base path for all routes, default is "_api_"
- `Authentication (Object)` : see Authentication section
- `Cache (Object)` : see Cache section
- `Cors (Object)` : see Cors section
- `RateLimiting (Object)` : see Rate limiting section
- `Csv (Object)` : see Csv section

## Authentication
Authentication is based on Jwt. You can configure how Jwt token should be interpreted : 
- `Authentication:Enabled (boolean)` : indicates if the Api should use authentication or not
- `Authentication:RequireHttpsMetadata (bool)` : indicates if HTTPS is required for the metadata address or authority, default : false
- `Authentication:ValidateIssuer (boolean)` : indicates if the Api should validate the issuer of the token, default : true
- `Authentication:Issuer (string)` : the issuer of the token
- `Authentication:ValidateIssuerSigningKey (boolean)` : indicates if the Signin key should be validated, default: true
- `Authentication:Key (string)` : the key to validate
- `Authentication:ValidateAudiance (boolean)` : indicates if the Api should validate the audience, default : false
- `Authentication:Audience (string)` : the audience to validate
- `Authentication:ValidateLifetime (boolean)` : indicates if the Api should validate the lifetime of the token, default false

## Cache
RestoDB can use a cache to be more performant and limit the requests to the database, to do so, use the 'Cache' section in configuration : 
- `Cache:Enabled (boolean)` : indicates if the Api should use cache or not, default: false
- `Cache:DurationInSeconds (int)` : indicates the cache duration in seconds, default: 60s
- `Cache:Absolute (boolean)` : indicates if the cache should be absolute(true) or sliding(false), default: true

## Cors
You can enable and configure Cors this way : 
- `Cors:Enabled (boolean)`: indicates if the Api should use Cors or not, default: false
- `Cors:AllowedOrigins (string[])`: the allowed origins
- `Cors:AllowedMethods (string[])`: the allowed methods
- `Cors:AllowedHeaders (string[])`: the allowed headers

## Rate limiting
You can enable and configure rate limiting this way : 
- `RateLimiter:Enabled (boolean)`: indicates if the Api should enable rate limiter, default: false
- `RateLimiter:PermitLimit (int)`: the number of requests allowed during the specified window, default: 100
- `RateLimiter:WindowInSeconds (int)`: the window in seconds, default: 60
- `RateLimiter:QueueLimit (int)`: the number of requests to be queued if the limit is reached, default: 0

## Csv
You can natively export get data in csv format by adding "/csv" to any route.
- `Csv:Enabled (boolean)`: indicates if the Api should enable Csv routes, default: true
- `Csv:Separator (string)`: the separator used in Csv results, default: "," (comma)
