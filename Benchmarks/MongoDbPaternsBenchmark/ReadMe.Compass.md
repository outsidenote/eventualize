# Compass

## Aggregation 

## By Id

$match
```json
{
  "_id": /^testdomain:testpartition:teststream:/,
  "_id": { $gte: "testdomain:testpartition:teststream:000_000_150_009" }
}
```

$sort
```json
{
  "_id": 1
}
```


## Compound

$match
```json
{
  "domain": "testdomain", 
    "partition": "testpartition", 
    "stream_id": "teststream", 
    "offset": { $gte: 1001 }
}
```

$sort
```json
{
  domain: 1,
  partition: 1,
  stream_id:1,
  offset: 1
}
```