# Application Insights for Kubernetes
Application Insights for Kubernetes is a monitoring solution that allows you to collect Application Insights telemetry pertaining to
incoming and outgoing requests to and from pods running in your Kubernetes cluster without the need for instrumenting the
application with an SDK. We utilize service mesh technology called Istio to collect data, so the only requirement is that your
Kubernetes deployment is configured to run with Istio (there are few simple requirements, see below for details).

*If you are not familiar with Application Insights, see [here](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview).*

Since service mesh lifts data off the wire, we can not intercept encrypted traffic. For traffic that doesn't leave the cluster,
use plain unencrypted protocol (e.g. HTTP). For external traffic that must be encrypted, consider setting up SSL termination at the ingress controller.

Applications running outside of the service mesh are not affected.

## Prerequisites
- a Kubernetes cluster
- console access to the cluster to run *kubectl*

## Installation steps
To enable the solution, we'll be performing the following steps:
- deploy the service mesh (if not already deployed)
- deploy the application (if not already deployed)
- ensure the application is part of the service mesh
- observe collected telemetry

### Deploy Istio
We are currently using Istio as the service mesh technology. If your cluster doesn't have Istio deployed, follow installation
instructions here: https://istio.io/docs/setup/kubernetes/.

### Configure your app to work with Istio
Istio supports two ways of instrumenting a pod (see [here](https://istio.io/docs/setup/kubernetes/additional-setup/sidecar-injection/)).
In most cases, it's easiest to simply mark the Kubernetes namespace containing your application with the *istio-injection* label:
```
kubectl label namespace <my-app-namespace> istio-injection=enabled
```

### Deploy your application
- Deploy your application to *my-app-namespace* namespace. If the application is already deployed, and you have followed the automatic
sidecar injection method described above, you need to recreate pods to ensure Istio injects its sidecar; either initiate a
rolling update or simply delete individual pods and wait for them to be recreated.
- Ensure your application complies with Istio requirements listed [here](https://istio.io/docs/setup/kubernetes/prepare/requirements/).

### Deploy Application Insights for Kubernetes using Kubernetes Manifests
1. Download and extract an *Application Insights for Kubernetes* release from [here](https://github.com/Microsoft/Application-Insights-Istio-Adapter/releases/).
2. Navigate to */src/kubernetes/* inside the release folder.
3. Edit *application-insights-istio-mixer-adapter-deployment.yaml*
    - edit the value of *ISTIO_MIXER_PLUGIN_AI_INSTRUMENTATIONKEY* environment variable to contain the instrumentation key of the Application Insights resource in Azure Portal to contain the telemetry.
    - *if required*, edit the value of *ISTIO_MIXER_PLUGIN_WATCHLIST_NAMESPACES* environment variable to contain a comma-separated list of namespaces for which you would like to enable monitoring. Leave it blank to
monitor all namespaces.
4. Apply *every* YAML file found under *src/kubernetes/* by running the following (you must still be inside */src/kubernetes/*):
   ```
   kubectl apply -f .
   ```

### Deploy Application Insights for Kubernetes using the included Helm Chart

[Helm](https://helm.sh/) is a Package Management tool for Kubernetes. Helm distributes software as Charts, which are packages of templated Kubernetes Manifests.

1. Ensure that you've installed the [Helm CLI](https://github.com/helm/helm#install) on your development machine.
2. Download and extract an *Application Insights for Kubernetes* release from [here](https://github.com/Microsoft/Application-Insights-Istio-Adapter/releases/).
3. Navigate to the `kubernetes-helm` directory.
4. Edit the `values.yaml` file, adding in your Application Insights Key, namespaces you'd like to watch, namespaces you'd like to ignore,
5. Run `helm template . > application-insights-istio-mixer.yaml`
6. Apply the generated yaml to your cluster: `kubectl apply -f application-insights-istio-mixer.yaml`

### Verify Application Insights for Kubernetes deployment
- Ensure Application Insights for Kubernetes adapter has been deployed:
  ```
  kubectl get pods -n istio-system -l "app=application-insights-istio-mixer-adapter"
  ```

#### Fine tuning: per-pod configuration
In some cases, finer tuning is required. To include or exclude telemetry for an individual pod from being collected,
use *appinsights/monitoring.enabled* label on that pod. This will have priority over all namespace-based configuration. Set *appinsights/monitoring.enabled* to *true* to include the pod, and to *false* to exclude it.

### View Application Insights telemetry
- issue requests against the application being monitored, or otherwise produce load on the application
- within 3-5 minutes you should start seeing telemetry appear in Azure Portal. Be sure to check out the *Application Map* section of your Application Insights resource in the Portal.

### Troubleshooting
Below is the troubleshooting flow to use when telemetry doesn't appear in Azure Portal as expected.
1. Ensure the application is under load and is sending/receiving requests in plain HTTP. Since telemetry is lifted off the wire, encrypted traffic is not supported. Of course, if there are no incoming or outgoing requests,
there will be no telemetry either.
2. Ensure the correct instrumentation key is provided in *ISTIO_MIXER_PLUGIN_AI_INSTRUMENTATIONKEY* environment variable in *application-insights-istio-mixer-adapter-deployment.yaml*. The instrumentation key
is found on the *Overview* blade of the Application Insights resource in Azure Portal.
3. Ensure the correct Kubernetes namespace is provided in *ISTIO_MIXER_PLUGIN_WATCHLIST_NAMESPACES* environment variable in *application-insights-istio-mixer-adapter-deployment.yaml*. Leave it blank to monitor all namespaces.
4. Ensure your application complies with Istio requirements listed [here](https://istio.io/docs/setup/kubernetes/prepare/requirements/).
5. Ensure your application's pods have been sidecar-injected by Istio. Verify that Istio's sidecar exists on each pod.
   ```
   kubectl describe pod -n <my-app-namespace> <my-app-pod-name>
   ```
   Verify that there is a container named *istio-proxy* running on the pod.

6. View *Application Insights for Kubernetes* adapter's traces.
   ```
   kubectl get pods -n istio-system -l "app=application-insights-istio-mixer-adapter"
   kubectl logs -n istio-system application-insights-istio-mixer-adapter-<fill in from previous command output>
   ```
   The count of received telemetry items is updated once a minute. If it doesn't grow minute over minute - no telemetry is being sent to the adapter by Istio.
   Look for any errors in the log.
7. If it has been established that *Application Insights for Kubernetes* adapter is not being fed telemetry, check Istio's Mixer logs to figure out why it's not sending data to the adapter:
   ```
   kubectl get pods -n istio-system -l "istio=mixer,app=telemetry"
   kubectl logs -n istio-system istio-telemetry-<fill in from previous command output> -c mixer
   ```
   Look for any errors, especially pertaining to communications with *applicationinsightsadapter* adapter.
8. To learn more about how Istio functions, please see documentation [here](https://istio.io/docs/concepts/what-is-istio/).

## FAQ
- *Are other service meshes supported?* We have supported Istio as our first service mesh; we are currently looking at others (Linkerd, Consul) and expect to expand support soon.
- *Can I separate telemetry between multiple instrumentation keys (Application Insights resources)?* Support for individually configurable instrumentation keys (by Kubernetes namespace) is coming soon.
As of right now, you can manually manipulate YAML configuration to set up multiple instances of the adapter and register them with Istio's Mixer. Each adapter can then have a separate configuration,
each containing its own namespace and instrumentation key.
- *Are Windows containers supported?* Since only Istio is supported currently, and Istio does not offer support for Windows containers - the answer is no. As soon as supported service meshes are compatible
with Windows containers this scenario will be enabled.
- *Distributed tracing support* Support for distributed tracing is coming soon. Currently, only Application Map is supported, not full distributed tracing chains.
- *Support for other protocols � Redis, mongoDB, HTTPS, etc.* Since telemetry is lifted off the wire, encrypted protocols (like HTTPS) cannot be supported. For internal traffic use plain unecnrypted protocols (like HTTP);
for external traffic that must be encrypted consider setting up SSL termination at the ingress gateway. Support for some of the most popular non-HTTP protocols is coming.

## Uninstall
To uninstall the product, run *kubectl delete -f <filename.yaml>* for *every* YAML file found under *src/kubernetes/*.
To uninstall Istio, follow instructions here: https://istio.io/docs/setup/kubernetes/install/helm/#uninstall.
