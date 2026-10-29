#!/usr/bin/env bash
# Rolls one environment (staging or production) to the images tagged $IMAGE_TAG
# (architecture.md section 8; runbook in docs/operations.md). Called by .github/workflows/deploy.yml.
#
# Order matters for zero-downtime deploys (expand → migrate → contract):
#   1. run the migration bundle as a one-off task and stop if it fails;
#   2. roll the API, worker and frontend services to the new images and wait until they are stable.
# ECS keeps the old tasks serving until the new ones pass their health checks, and rolls back
# by itself if they never do (deployment circuit breaker).
#
# Needs: AWS credentials, and these variables (set per GitHub environment):
#   AWS_REGION, ECR_REGISTRY, ECS_CLUSTER, ECS_SUBNETS (comma-separated), ECS_SECURITY_GROUP, IMAGE_TAG
set -euo pipefail

: "${AWS_REGION:?}" "${ECR_REGISTRY:?}" "${ECS_CLUSTER:?}" "${ECS_SUBNETS:?}" "${ECS_SECURITY_GROUP:?}" "${IMAGE_TAG:?}"

# Registers a copy of the task definition `family` with each container's image moved to IMAGE_TAG,
# and prints the new revision's ARN. Everything else (CPU, secrets, logging) stays as provisioned.
register_with_new_image() {
  local family="$1"
  aws ecs describe-task-definition --task-definition "$family" --query taskDefinition --output json \
    | jq --arg tag "$IMAGE_TAG" '
        .containerDefinitions |= map(.image = (.image | sub(":[^:/]+$"; "") + ":" + $tag))
        | del(.taskDefinitionArn, .revision, .status, .requiresAttributes, .compatibilities,
              .registeredAt, .registeredBy)' \
    > "/tmp/$family.json"
  aws ecs register-task-definition --cli-input-json "file:///tmp/$family.json" \
    --query taskDefinition.taskDefinitionArn --output text
}

echo "Running migrations ($IMAGE_TAG)"
migrations=$(register_with_new_image nclex-migrations)
task=$(aws ecs run-task --cluster "$ECS_CLUSTER" --launch-type FARGATE --task-definition "$migrations" \
  --network-configuration "awsvpcConfiguration={subnets=[$ECS_SUBNETS],securityGroups=[$ECS_SECURITY_GROUP],assignPublicIp=DISABLED}" \
  --query 'tasks[0].taskArn' --output text)
aws ecs wait tasks-stopped --cluster "$ECS_CLUSTER" --tasks "$task"
exit_code=$(aws ecs describe-tasks --cluster "$ECS_CLUSTER" --tasks "$task" \
  --query 'tasks[0].containers[0].exitCode' --output text)
if [ "$exit_code" != "0" ]; then
  echo "Migrations failed (exit code $exit_code); nothing was deployed. See the nclex-migrations logs." >&2
  exit 1
fi

for service in nclex-api nclex-worker nclex-web; do
  echo "Deploying $service ($IMAGE_TAG)"
  aws ecs update-service --cluster "$ECS_CLUSTER" --service "$service" \
    --task-definition "$(register_with_new_image "$service")" > /dev/null
done

echo "Waiting for the services to become stable"
aws ecs wait services-stable --cluster "$ECS_CLUSTER" --services nclex-api nclex-worker nclex-web
echo "Deployed $IMAGE_TAG"
