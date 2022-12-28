# Firepuma.WebPush

## Introduction

This solution was generated with [francoishill/Firepuma.Template.GoogleCloudRunService](https://github.com/francoishill/Firepuma.Template.GoogleCloudRunService).

The following projects were generated as part of the solution:

* Firepuma.WebPush.Domain project contains the domain logic (not tightly coupled to Mongo or other infrastructure specifics)
* Firepuma.WebPush.Infrastructure contains infrastructure code, like mongo repositories inheriting from `MongoDbRepository<T>`
* Firepuma.WebPush.Tests contains unit tests
* Firepuma.WebPush.Worker project contains the service that will get deployed to Google Cloud Run

---

## Deploying

When using github, the deployment will happen automatically due to the folder containing workflow yaml files in the `.github/workflows` folder.

To test locally whether the Dockerfile can build, run the following command:

```shell
docker build --tag tmp-test-webpush-service --file Firepuma.WebPush.Worker/Dockerfile .
```

Run `npx web-push generate-vapid-keys` to generate public and private keypair for WebPush.