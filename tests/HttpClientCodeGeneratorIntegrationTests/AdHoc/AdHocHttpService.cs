using HttpClientGenerator.Shared;
using System.Threading.Tasks;

namespace HttpClientCodeGeneratorIntegrationTests.AdHoc
{
    public partial class AdHocHttpService
    {
        [HttpRequestHeader("x-foo", "x-bar")]
        [HttpGet("ad-hoc/headers-check")]
        public partial Task CheckHeadersAsync();
    }
}
