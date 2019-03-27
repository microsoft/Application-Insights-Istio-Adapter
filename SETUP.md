# Application Insights for AKS

## Prerequisites
- an AKS cluster (Kubernetes cluster in Azure)
- console access to the AKS cluster to run kubectl

## Installation steps

### Deploy Istio
- Follow instructions here: https://istio.io/docs/setup/kubernetes/.

### Configure your namespace for Istio sidecar autoinjection
- For the namespace that contains your application(s) which need(s) to be monitored, add the *istio-injection* label
  ```
  kubectl label namespace my-app-namespace istio-injection=enabled
  ```

### Deploy your application
- Deploy your application to *my-app-namespace* namespace. If the application is already deployed, you need to recreate pods to ensure Istio injects its sidecar; either initiate a rolling update or simply delete individual pods and wait for them to be recreated.
- Ensure your application complies with Istio requirements listed here: https://istio.io/docs/setup/kubernetes/prepare/requirements/

### Deploy Application Insights for AKS
1. Download and extract an Application Insights for AKS release from here: https://github.com/Microsoft/Application-Insights-Istio-Adapter/releases/
2. Navigate to */src/kubernetes/* inside the release folder.
3. Edit *application-insights-istio-mixer-adapter-deployment.yaml*
    - edit the value of *ISTIO_MIXER_PLUGIN_AI_INSTRUMENTATIONKEY* environment variable to contain the instrumentation key of the Application Insights resource in Azure portal to contain the telemetry.
    - edit the value of *ISTIO_MIXER_PLUGIN_WATCHLIST_NAMESPACES* environment variable to contain a comma-separated list of namespaces for which you would like to enable monitoring.
4. Run *kubectl apply -f <filename.yaml>* on *every* YAML file found under *src/kubernetes/*.

## Uninstall
To uninstall the product completely, run *kubectl delete -f <filename.yaml>* on *every* YAML file found under *src/kubernetes/*.
