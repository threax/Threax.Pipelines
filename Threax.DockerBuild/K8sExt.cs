using k8s;
using k8s.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.K8sDeploy
{
    /// <summary>
    /// The k8s c# api is terrible. It uses exceptions for flow control. These extension methods mask this for replacing k8s objects.
    /// </summary>
    public static class K8sExt
    {
        public static V1Secret CreateOrReplaceNamespacedSecret(this IKubernetes operations, V1Secret body, string namespaceParameter)
        {
            ExecuteIgnoreNotFound(() => operations.DeleteNamespacedSecret(body.Metadata.Name, namespaceParameter));
            return operations.CreateNamespacedSecret(body, namespaceParameter);
        }

        public static V1ConfigMap CreateOrReplaceNamespacedConfigMap(this IKubernetes operations, V1ConfigMap body, string namespaceParameter)
        {
            ExecuteIgnoreNotFound(() => operations.DeleteNamespacedConfigMap(body.Metadata.Name, namespaceParameter));
            return operations.CreateNamespacedConfigMap(body, namespaceParameter);
        }

        public static V1Deployment CreateOrReplaceNamespacedDeployment(this IKubernetes operations, V1Deployment body, string namespaceParameter)
        {
            ExecuteIgnoreNotFound(() => operations.DeleteNamespacedDeployment(body.Metadata.Name, namespaceParameter));
            return operations.CreateNamespacedDeployment(body, namespaceParameter);
        }

        public static V1Service CreateOrReplaceNamespacedService(this IKubernetes operations, V1Service body, string namespaceParameter)
        {
            ExecuteIgnoreNotFound(() => operations.DeleteNamespacedService(body.Metadata.Name, namespaceParameter));
            return operations.CreateNamespacedService(body, namespaceParameter);
        }

        public static Networkingv1beta1Ingress CreateOrReplaceNamespacedIngress1(this IKubernetes operations, Networkingv1beta1Ingress body, string namespaceParameter)
        {
            ExecuteIgnoreNotFound(() => operations.DeleteNamespacedIngress1(body.Metadata.Name, namespaceParameter));
            return operations.CreateNamespacedIngress1(body, namespaceParameter);
        }

        private static void ExecuteIgnoreNotFound(Action action)
        {
            try
            {
                action();
            }
            catch (HttpOperationException ex)
            {
                if (ex.Response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    //Can get 404 from the api, this should be ignored since it just means there is no target object
                    //If we get anything else, rethrow the exception
                    throw;
                }
            }
        }
    }
}
