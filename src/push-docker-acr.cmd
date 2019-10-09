; %1 is the id of the image being pushed, %2 is the tag to push with
docker tag %1 applicationinsights.azurecr.io/public/applicationinsights/istiomixeradapter:%2
docker push applicationinsights.azurecr.io/public/applicationinsights/istiomixeradapter:%2