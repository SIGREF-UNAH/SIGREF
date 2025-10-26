using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using SIGREF.API.Constants;
using SIGREF.API.Database;
using SIGREF.API.Dtos.AuditLog;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;

namespace SIGREF.API.Services.AuditLog
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IMongoCollection<Database.AuditLog> _auditLogsCollection;
        private readonly ILogger<AuditLogService> _logger;
        private readonly FhirClient _fhirClient;
        private readonly string _fhirServerUrl;

        public AuditLogService(IOptions<Env> env, ILogger<AuditLogService> logger)
        {
            _logger = logger;
            var mongoConfig = env.Value.MongoDB;
            _fhirServerUrl = env.Value.Fhir.BaseUrl;

            var settings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
                Timeout = 30000
            };
            _fhirClient = new FhirClient(_fhirServerUrl, settings);

            var mongoClient = new MongoClient(mongoConfig.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoConfig.DatabaseName);
            _auditLogsCollection = mongoDatabase.GetCollection<Database.AuditLog>(mongoConfig.AuditLogsCollection);

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var indexKeysDefinition = Builders<Database.AuditLog>.IndexKeys
                    .Descending("Timestamp");
                _auditLogsCollection.Indexes.CreateOne(new CreateIndexModel<Database.AuditLog>(indexKeysDefinition));

                var userIndexKeys = Builders<Database.AuditLog>.IndexKeys
                    .Ascending("UserId");
                _auditLogsCollection.Indexes.CreateOne(new CreateIndexModel<Database.AuditLog>(userIndexKeys));

                var actionTypeIndexKeys = Builders<Database.AuditLog>.IndexKeys
                    .Ascending("ActionType");
                _auditLogsCollection.Indexes.CreateOne(new CreateIndexModel<Database.AuditLog>(actionTypeIndexKeys));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al crear índices de MongoDB (pueden ya existir)");
            }
        }

        public async System.Threading.Tasks.Task LogActionAsync(string userId, string username, string action, string actionType,
            string? endpoint = null, string? httpMethod = null, int? statusCode = null,
            string? errorMessage = null, string? requestBody = null, string? responseBody = null,
            string? ipAddress = null, string? userAgent = null, string? additionalData = null)
        {
            try
            {
                _logger.LogInformation($"[AuditLog] Iniciando registro: User={username}, Action={actionType}, Endpoint={endpoint}");

                var fhirAuditEvent = await CreateFhirAuditEventAsync(
                    userId, username, action, actionType, endpoint, httpMethod,
                    statusCode, errorMessage, requestBody, responseBody,
                    ipAddress, userAgent, additionalData);

                _logger.LogInformation("[AuditLog] AuditEvent FHIR creado, intentando enviar a servidor FHIR...");

                var sentToFhir = await SendAuditEventToFhirAsync(fhirAuditEvent);

                _logger.LogInformation($"[AuditLog] Resultado envío FHIR: {sentToFhir}, procediendo a guardar en MongoDB...");

                var auditLog = new Database.AuditLog
                {
                    UserId = userId,
                    Username = username,
                    Action = action,
                    ActionType = actionType,
                    Endpoint = endpoint,
                    HttpMethod = httpMethod,
                    StatusCode = statusCode,
                    ErrorMessage = errorMessage,
                    RequestBody = requestBody,
                    ResponseBody = responseBody,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow,
                    AdditionalData = additionalData
                };

                await _auditLogsCollection.InsertOneAsync(auditLog);

                _logger.LogInformation($"[AuditLog] ✓ Log guardado exitosamente en MongoDB - ID: {auditLog.Id}");

                if (sentToFhir)
                {
                    _logger.LogInformation("AuditEvent enviado exitosamente a FHIR y guardado en MongoDB");
                }
                else
                {
                    _logger.LogWarning("AuditEvent guardado solo en MongoDB (FHIR no disponible)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AuditLog] ✗ ERROR al guardar log de auditoría - User={username}, Action={actionType}, Endpoint={endpoint}");
            }
        }

        public async System.Threading.Tasks.Task<List<AuditLogDto>> GetLogsAsync(AuditLogFilterDto filter)
        {
            filter.NormalizeDates();

            var filterBuilder = Builders<Database.AuditLog>.Filter;
            var filters = new List<FilterDefinition<Database.AuditLog>>();

            if (!string.IsNullOrEmpty(filter.UserId))
                filters.Add(filterBuilder.Eq("UserId", filter.UserId));

            if (!string.IsNullOrEmpty(filter.Username))
                filters.Add(filterBuilder.Regex("Username", new MongoDB.Bson.BsonRegularExpression(filter.Username, "i")));

            if (!string.IsNullOrEmpty(filter.ActionType))
                filters.Add(filterBuilder.Eq("ActionType", filter.ActionType));

            if (filter.StartDate.HasValue)
                filters.Add(filterBuilder.Gte("Timestamp", filter.StartDate.Value));

            if (filter.EndDate.HasValue)
                filters.Add(filterBuilder.Lte("Timestamp", filter.EndDate.Value));

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            var sortDefinition = Builders<Database.AuditLog>.Sort.Descending("Timestamp");

            var logs = await _auditLogsCollection
                .Find(combinedFilter)
                .Sort(sortDefinition)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return logs.Select(l => new AuditLogDto
            {
                Id = l.Id.ToString(),   
                UserId = l.UserId,
                Username = l.Username,
                Action = l.Action,
                ActionType = l.ActionType,
                Endpoint = l.Endpoint,
                HttpMethod = l.HttpMethod,
                StatusCode = l.StatusCode,
                ErrorMessage = l.ErrorMessage,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                Timestamp = l.Timestamp,
                AdditionalData = l.AdditionalData
            }).ToList();
        }

        public async System.Threading.Tasks.Task<AuditLogDto?> GetLogByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return null;

            var filter = Builders<Database.AuditLog>.Filter.Eq("_id", objectId);

            var log = await _auditLogsCollection
                .Find(filter)
                .FirstOrDefaultAsync();

            if (log == null)
                return null;

            return new AuditLogDto
            {
                Id = log.Id.ToString(),
                UserId = log.UserId,
                Username = log.Username,
                Action = log.Action,
                ActionType = log.ActionType,
                Endpoint = log.Endpoint,
                HttpMethod = log.HttpMethod,
                StatusCode = log.StatusCode,
                ErrorMessage = log.ErrorMessage,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                Timestamp = log.Timestamp,
                AdditionalData = log.AdditionalData
            };
        }

        public async System.Threading.Tasks.Task<AuditEvent> CreateFhirAuditEventAsync(string userId, string username, string action,
            string actionType, string? endpoint = null, string? httpMethod = null,
            int? statusCode = null, string? errorMessage = null, string? requestBody = null,
            string? responseBody = null, string? ipAddress = null, string? userAgent = null,
            string? additionalData = null)
        {
            var auditEvent = new AuditEvent
            {
                Type = new Coding
                {
                    System = "http://terminology.hl7.org/CodeSystem/audit-event-type",
                    Code = DetermineAuditEventTypeCode(actionType),
                    Display = DetermineAuditEventTypeDisplay(actionType)
                },

                Subtype = new List<Coding>
                {
                    new Coding
                    {
                        System = "http://hl7.org/fhir/restful-interaction",
                        Code = DetermineRestfulInteraction(httpMethod),
                        Display = httpMethod
                    }
                },

                Action = DetermineFhirAction(httpMethod),
                RecordedElement = new Instant(DateTimeOffset.UtcNow),
                Outcome = DetermineFhirOutcome(statusCode, errorMessage),
                OutcomeDesc = errorMessage ?? (statusCode.HasValue && statusCode >= 200 && statusCode < 300 ? "Success" : "Error"),

                Agent = new List<AuditEvent.AgentComponent>
                {
                    new AuditEvent.AgentComponent
                    {
                        Type = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/v3-ParticipationType",
                                    Code = "AUT",
                                    Display = "Author"
                                }
                            }
                        },

                        Who = new ResourceReference
                        {
                            Identifier = new Identifier
                            {
                                Value = userId,
                                System = "urn:oid:2.16.840.1.113883.4.6"
                            },
                            Display = username
                        },

                        Requestor = true,

                        Network = ipAddress != null ? new AuditEvent.NetworkComponent
                        {
                            Address = ipAddress,
                            Type = AuditEvent.AuditEventAgentNetworkType.N1
                        } : null
                    }
                },

                Source = new AuditEvent.SourceComponent
                {
                    Site = "SIGREF API",
                    Observer = new ResourceReference
                    {
                        Display = "SIGREF Backend System"
                    },
                    Type = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://terminology.hl7.org/CodeSystem/security-source-type",
                            Code = "4",
                            Display = "Application Server"
                        }
                    }
                },

                Entity = CreateEntityComponents(endpoint, requestBody, responseBody, additionalData)
            };

            return auditEvent;
        }

        public async System.Threading.Tasks.Task<bool> SendAuditEventToFhirAsync(AuditEvent auditEvent)
        {
            try
            {
                if (string.IsNullOrEmpty(_fhirServerUrl))
                {
                    _logger.LogWarning("URL del servidor FHIR no configurada. AuditEvent no enviado.");
                    return false;
                }

                var result = await _fhirClient.CreateAsync(auditEvent);

                if (result != null && !string.IsNullOrEmpty(result.Id))
                {
                    _logger.LogInformation($"AuditEvent creado en FHIR con ID: {result.Id}");
                    return true;
                }

                return false;
            }
            catch (FhirOperationException fhirEx)
            {
                _logger.LogError(fhirEx, $"Error FHIR al enviar AuditEvent: {fhirEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar AuditEvent al servidor FHIR");
                return false;
            }
        }

        #region Helper Methods

        private string DetermineAuditEventTypeCode(string actionType)
        {
            return actionType?.ToUpper() switch
            {
                "LOGIN" => "110114",
                "LOGOUT" => "110123",
                "CREATE" => "rest",
                "READ" => "rest",
                "UPDATE" => "rest",
                "DELETE" => "rest",
                "ERROR" => "rest",
                _ => "rest"
            };
        }

        private string DetermineAuditEventTypeDisplay(string actionType)
        {
            return actionType?.ToUpper() switch
            {
                "LOGIN" => "User Authentication",
                "LOGOUT" => "Logout",
                "CREATE" => "RESTful Operation",
                "READ" => "RESTful Operation",
                "UPDATE" => "RESTful Operation",
                "DELETE" => "RESTful Operation",
                "ERROR" => "RESTful Operation",
                _ => "RESTful Operation"
            };
        }

        private string DetermineRestfulInteraction(string? httpMethod)
        {
            return httpMethod?.ToUpper() switch
            {
                "GET" => "read",
                "POST" => "create",
                "PUT" => "update",
                "PATCH" => "patch",
                "DELETE" => "delete",
                _ => "read"
            };
        }

        private AuditEvent.AuditEventAction? DetermineFhirAction(string? httpMethod)
        {
            return httpMethod?.ToUpper() switch
            {
                "GET" => AuditEvent.AuditEventAction.R,
                "POST" => AuditEvent.AuditEventAction.C,
                "PUT" => AuditEvent.AuditEventAction.U,
                "PATCH" => AuditEvent.AuditEventAction.U,
                "DELETE" => AuditEvent.AuditEventAction.D,
                _ => AuditEvent.AuditEventAction.R
            };
        }

        private AuditEvent.AuditEventOutcome DetermineFhirOutcome(int? statusCode, string? errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage) || (statusCode.HasValue && statusCode >= 500))
                return AuditEvent.AuditEventOutcome.N8;

            if (statusCode.HasValue && statusCode >= 400)
                return AuditEvent.AuditEventOutcome.N4;

            if (statusCode.HasValue && statusCode >= 200 && statusCode < 300)
                return AuditEvent.AuditEventOutcome.N0;

            return AuditEvent.AuditEventOutcome.N0;
        }

        private List<AuditEvent.EntityComponent> CreateEntityComponents(string? endpoint,
            string? requestBody, string? responseBody, string? additionalData)
        {
            var entities = new List<AuditEvent.EntityComponent>();

            if (!string.IsNullOrEmpty(endpoint))
            {
                var entity = new AuditEvent.EntityComponent
                {
                    What = new ResourceReference
                    {
                        Display = endpoint
                    },
                    Type = new Coding
                    {
                        System = "http://terminology.hl7.org/CodeSystem/audit-entity-type",
                        Code = "2",
                        Display = "System Object"
                    },
                    Role = new Coding
                    {
                        System = "http://terminology.hl7.org/CodeSystem/object-role",
                        Code = "4",
                        Display = "Domain Resource"
                    },
                    Name = endpoint
                };

                if (!string.IsNullOrEmpty(requestBody))
                {
                    entity.Detail = entity.Detail ?? new List<AuditEvent.DetailComponent>();
                    entity.Detail.Add(new AuditEvent.DetailComponent
                    {
                        Type = "RequestBody",
                        Value = new FhirString(requestBody.Length > 1000 ? requestBody.Substring(0, 1000) + "..." : requestBody)
                    });
                }

                if (!string.IsNullOrEmpty(responseBody))
                {
                    entity.Detail = entity.Detail ?? new List<AuditEvent.DetailComponent>();
                    entity.Detail.Add(new AuditEvent.DetailComponent
                    {
                        Type = "ResponseBody",
                        Value = new FhirString(responseBody.Length > 1000 ? responseBody.Substring(0, 1000) + "..." : responseBody)
                    });
                }

                if (!string.IsNullOrEmpty(additionalData))
                {
                    entity.Detail = entity.Detail ?? new List<AuditEvent.DetailComponent>();
                    entity.Detail.Add(new AuditEvent.DetailComponent
                    {
                        Type = "AdditionalData",
                        Value = new FhirString(additionalData)
                    });
                }

                entities.Add(entity);
            }

            return entities;
        }

        public Task<AuditLogDto> GetLogByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}