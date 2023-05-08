# RestoDB
RestoDB (Rest over database) is a micro-service that exposes a database through REST.
You can expose all tables and vues, or select which ones you want to be exposed.

## Providers
RestoDB supports SQL Server and PostgreSQL.
MySql and Sqlite will be implemented soon.

## Config
- `LimitTo (string[])` : entities allowed to expose. if null or empty, all entities in the database will be exposed
- `DbProvider (string)` : the database provider that will be used. Possible values are 'NpgSql' or 'SqlServer'
- `EnableSwagger (boolean)` : indicates if the swagger should be enabled or not
- `ApiSegment (string)` : the base path for all routes, default is "api"
- `Authentication (Object)` : see Authentication section
- `Cache (Object)` : see Cache section

## Authentication
Authentication is based on Jwt. You can configure how Jwt token should be interpreted : 
- `Authentication.Enabled (boolean)` : indicates if the Api should use authentication or not
- `Authentication.RequireHttpsMetadata (bool)` : indicates if HTTPS is required for the metadata address or authority, default : false
- `Authentication.ValidateIssuer (boolean)` : indicates if the Api should validate the issuer of the token, default : true
- `Authentication.Issuer (string)` : the issuer of the token
- `Authentication.ValidateIssuerSigningKey (boolean)` : indicates if the Signin key should be validated, default: true
- `Authentication.Key (string)` : the key to validate
- `Authentication.ValidateAudiance (boolean)` : indicates if the Api should validate the audience, default : false
- `Authentication.Audience (string)` : the audience to validate
- `Authentication.ValidateLifetime (boolean)` : indicates if the Api should validate the lifetime of the token, default false

## Cache
RestoDB can use a cache to be more performant, to do so, use the 'Cache' section in configuration : 
- `Cache.Enabled (boolean)` : indicated if the Api should use cache or not, default: false
- `Cache.DurationInSeconds (int)` : indicates the cache duration in seconds, default: 60s
- `Cache.Absolute (boolean)` : indicates if the cache should be absolute(true) or sliding(false), default: true

## Todo
- ~~Jwt authentication~~
- ~~Swagger~~
- ~~logs~~
- ~~ensure it works for vues~~
- ~~add odata (or other way(s) to query ?)~~
- add api rate limiting
- add CORS
- add CI/CD
- Crud operations ?
- Dockerize
- Write a decent readme (in progress)
- ~~Add a cache~~
- Add more database providers
