using System.Collections;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;

namespace MushroomMapApp.Shared.Response;

public static class ApiResponse
{
    public static IActionResult Ok<TModel>(TModel? data,
            string apiMessage = "Request succeeded",
            Action<dynamic>? metaDataAction = null,
            Action<object, dynamic>? itemMetaDataAction = null)
        {
            dynamic? metaData = null;
            if (metaDataAction != null)
            {
                metaData = new ExpandoObject();
                metaDataAction(metaData);
            }

            if (data is IEnumerable enumerable && itemMetaDataAction != null)
            {
                var items = enumerable.Cast<object>();

                var itemsWithMeta = items.Select(item =>
                {
                    dynamic itemMeta = null;
                    itemMeta = new ExpandoObject();
                    itemMetaDataAction(item, itemMeta);

                    return new ItemWithMeta<object>
                    {
                        Data = item,
                        Meta = itemMeta
                    };
                }).ToList();
                    var listResponse = new Response<List<ItemWithMeta<object>>>(itemsWithMeta)
                    {
                        MetaData = metaData,
                        Message = apiMessage,
                        Success = true
                    };

                    return new OkObjectResult(listResponse);

            }


            var response = new Response<TModel>(data)
            {
                Message = apiMessage,
                Success = true,
                MetaData = metaData
            };

            return new OkObjectResult(response);
        }

        public static IActionResult Ok(string apiMessage = "Request succeeded", Action<dynamic>? metaDataAction = null)
        {
            return Ok<object>(null, apiMessage, metaDataAction);
        }

        public static IActionResult NoContent(string apiMessage = "No content", Action<dynamic>? metaDataAction = null)
        {
            dynamic? metaData = null;
            if (metaDataAction != null)
            {
                metaData = new ExpandoObject();
                metaDataAction(metaData);
            }

            var response = new ErrorResponse(apiMessage, metaData);

            return new NotFoundObjectResult(response);
        }

        public static IActionResult Forbidden(string apiMessage = "Forbidden api request", Action<dynamic>? metaDataAction = null)
        {
            dynamic? metaData = null;
            if (metaDataAction != null)
            {
                metaData = new ExpandoObject();
                metaDataAction(metaData);
            }
            
            var response = new ErrorResponse(apiMessage, metaData);

            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        public static IActionResult BadRequest(string apiMessage = "Bad api request", string[] errors = null, Action<dynamic>? metaDataAction = null)
        {
            dynamic? metaData = null;
            if (metaDataAction != null)
            {
                metaData = new ExpandoObject();
                metaDataAction(metaData);
            }
            
            var response = new  ErrorResponse(apiMessage, errors, metaData);

            return new BadRequestObjectResult(response);
        }

        public static IActionResult NotFound(string apiMessage = "Resource not found", Action<dynamic>? metaDataAction = null)
        {
            dynamic? metaData = null;
            if (metaDataAction != null)
            {
                metaData = new ExpandoObject();
                metaDataAction(metaData);
            }
            
            var response = new ErrorResponse(apiMessage, metaData);

            return new NotFoundObjectResult(response);
        }

        public static IActionResult InternalServerError(string apiMessage = "Internal server error", string[] errors = null, Action<dynamic>? metaDataAction = null)
        {
            dynamic? metaData = null;
            if (metaDataAction != null)
            {
                metaData = new ExpandoObject();
                metaDataAction(metaData);
            }
            
            var response = new ErrorResponse(apiMessage, errors, metaData);

            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
}