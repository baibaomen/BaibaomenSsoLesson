using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OneKeyRunner
{
    class Program
    {
        static string RootFolderPath = AppDomain.CurrentDomain.BaseDirectory;

        static void Main(string[] args)
        {

            var hostFile = Path.Combine(Environment.SystemDirectory, "drivers\\etc\\hosts");
            var hostContent = File.ReadAllText(hostFile);
            if (!hostContent.Contains("sso.baibaomen.com"))
            {
                Console.WriteLine("正在添加演示域名到host文件");
                var toAppend = @"
127.0.0.1 sso.baibaomen.com
127.0.0.1 java-client.net
127.0.0.1 php-client.cn";
                File.AppendAllText(hostFile, toAppend);
            }
            else
            {
                Console.WriteLine("演示域名已存在于host文件中，跳过添加步骤");
            }

            Console.WriteLine("正在安装证书");

            string certFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wildcard.baibaomen.com.pfx");
            var cert = new X509Certificate2(File.ReadAllBytes(certFile), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

            InstallCert(new X509Store(StoreName.Root, StoreLocation.LocalMachine), cert);

            InstallCert(new X509Store(StoreName.My, StoreLocation.LocalMachine), cert);

              ServerManager iisManager = new ServerManager();

            var smSite = iisManager.Sites.FirstOrDefault(s => s.Name.Contains("sso.baibaomen.com"));

            if (smSite == null)
            {

                Console.WriteLine("正在创建IIS站点");

                var sites = new string[] { "sso.baibaomen.com", "java-client.net", "php-client.cn" };

                foreach (var site in sites)
                {
                    ApplicationPool newPool = iisManager.ApplicationPools.Add(site);
                    newPool.ManagedRuntimeVersion = "v4.0";
                    newPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                }

                iisManager.CommitChanges();

                foreach (var site in sites)
                {
                    iisManager.Sites.Add(site, "http", "*:80:" + site , Path.Combine(RootFolderPath, "Lesson1\\" + site));
                    var theSite = iisManager.Sites.FirstOrDefault(s => s.Name.Equals(site));
                    theSite.ApplicationDefaults.ApplicationPoolName = site;
                }

                iisManager.CommitChanges();

                smSite = iisManager.Sites.FirstOrDefault(s => s.Name.Equals("sso.baibaomen.com"));
                Binding binding = smSite.Bindings.CreateElement("binding");
                binding["protocol"] = "https";
                binding["certificateHash"] = cert.GetCertHashString();
                binding["certificateStoreName"] = "Root";
                binding["bindingInformation"] = "*:443:sso.baibaomen.com";
                smSite.Bindings.Add(binding);
                iisManager.CommitChanges();
            }
            else
            {
                Console.WriteLine("IIS站点已存在，跳过创建");
            }

            Console.WriteLine("正在打开站点页面");

            var p = Process.Start("https://sso.baibaomen.com");
            p = Process.Start("http://java-client.net");
            p = Process.Start("http://php-client.cn");
        }

        private static void InstallCert(X509Store store, X509Certificate2 cert)
        {
            store.Open(OpenFlags.ReadWrite);
            var certs = store.Certificates;
            var isInstalled = false;
            foreach (X509Certificate2 storeCert in certs)
            {
                if (storeCert.Issuer == "C=US, OU=baibaomen.com, O=baibaomen.com, CN=*.baibaomen.com")
                {
                    isInstalled = true;
                    break;
                }
            }

            if (!isInstalled)
            {
                store.Add(cert);
                store.Close();
            }
        }
    }
}
