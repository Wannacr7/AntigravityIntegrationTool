using System;
using System.Security.Cryptography;
using System.Text;

namespace Antigravity.Editor
{
    public static class SolutionGuidGenerator
    {
        public static string GuidForProject(string projectName)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(projectName));
                return new Guid(hash).ToString("B").ToUpper();
            }
        }

        public static string GuidForSolution(string solutionName)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(solutionName + "_Solution"));
                return new Guid(hash).ToString("B").ToUpper();
            }
        }
    }
}
