<a name='contents'></a>
# Contents [#](#contents 'Go To Here')

- [RealmDoesJson](#T-RealmJson-Extensions-RealmDoesJson 'RealmJson.Extensions.RealmDoesJson')
  - [ExMalFormeJsonMessage](#F-RealmJson-Extensions-RealmDoesJson-ExMalFormeJsonMessage 'RealmJson.Extensions.RealmDoesJson.ExMalFormeJsonMessage')
  - [CreateAllFromJson(realm,stream)](#M-RealmJson-Extensions-RealmDoesJson-CreateAllFromJson-Realms-Realm,System-IO-Stream- 'RealmJson.Extensions.RealmDoesJson.CreateAllFromJson(Realms.Realm,System.IO.Stream)')
  - [CreateAllFromJson(realm,jsonString)](#M-RealmJson-Extensions-RealmDoesJson-CreateAllFromJson-Realms-Realm,System-String- 'RealmJson.Extensions.RealmDoesJson.CreateAllFromJson(Realms.Realm,System.String)')
  - [CreateObjectFromJson(realm,jsonString)](#M-RealmJson-Extensions-RealmDoesJson-CreateObjectFromJson-Realms-Realm,System-String- 'RealmJson.Extensions.RealmDoesJson.CreateObjectFromJson(Realms.Realm,System.String)')
  - [CreateObjectFromJson(realm,stream)](#M-RealmJson-Extensions-RealmDoesJson-CreateObjectFromJson-Realms-Realm,System-IO-Stream- 'RealmJson.Extensions.RealmDoesJson.CreateObjectFromJson(Realms.Realm,System.IO.Stream)')
  - [CreateOrUpdateObjectFromJson(realm,jsonString)](#M-RealmJson-Extensions-RealmDoesJson-CreateOrUpdateObjectFromJson-Realms-Realm,System-String- 'RealmJson.Extensions.RealmDoesJson.CreateOrUpdateObjectFromJson(Realms.Realm,System.String)')

<a name='assembly'></a>
# RealmJson.Extensions [#](#assembly 'Go To Here') [=](#contents 'Back To Contents')

<a name='T-RealmJson-Extensions-RealmDoesJson'></a>
## RealmDoesJson [#](#T-RealmJson-Extensions-RealmDoesJson 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

RealmJson.Extensions

##### Summary

.Net Realm does json.

<a name='F-RealmJson-Extensions-RealmDoesJson-ExMalFormeJsonMessage'></a>
### ExMalFormeJsonMessage `constants` [#](#F-RealmJson-Extensions-RealmDoesJson-ExMalFormeJsonMessage 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Malformed json exception message.

<a name='M-RealmJson-Extensions-RealmDoesJson-CreateAllFromJson-Realms-Realm,System-IO-Stream-'></a>
### CreateAllFromJson(realm,stream) `method` [#](#M-RealmJson-Extensions-RealmDoesJson-CreateAllFromJson-Realms-Realm,System-IO-Stream- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Creates multiple RealmObjects from a json stream.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| realm | [Realms.Realm](#T-Realms-Realm 'Realms.Realm') | Realm Instance. |
| stream | [System.IO.Stream](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IO.Stream 'System.IO.Stream') | Stream. |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | RealmObject-based Class. |

<a name='M-RealmJson-Extensions-RealmDoesJson-CreateAllFromJson-Realms-Realm,System-String-'></a>
### CreateAllFromJson(realm,jsonString) `method` [#](#M-RealmJson-Extensions-RealmDoesJson-CreateAllFromJson-Realms-Realm,System-String- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Creates multiple RealmObjects from a json stream.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| realm | [Realms.Realm](#T-Realms-Realm 'Realms.Realm') | Realm Instance. |
| jsonString | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Json string. |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | RealmObject-based Class. |

<a name='M-RealmJson-Extensions-RealmDoesJson-CreateObjectFromJson-Realms-Realm,System-String-'></a>
### CreateObjectFromJson(realm,jsonString) `method` [#](#M-RealmJson-Extensions-RealmDoesJson-CreateObjectFromJson-Realms-Realm,System-String- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Creates a single RealmObject from a json string.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| realm | [Realms.Realm](#T-Realms-Realm 'Realms.Realm') | Realm Instance |
| jsonString | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Json string |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | RealmOject-based Class.. |

<a name='M-RealmJson-Extensions-RealmDoesJson-CreateObjectFromJson-Realms-Realm,System-IO-Stream-'></a>
### CreateObjectFromJson(realm,stream) `method` [#](#M-RealmJson-Extensions-RealmDoesJson-CreateObjectFromJson-Realms-Realm,System-IO-Stream- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Creates the single RealmObject from a stream containing json.

##### Returns

The object from json.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| realm | [Realms.Realm](#T-Realms-Realm 'Realms.Realm') | Realm Instance. |
| stream | [System.IO.Stream](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IO.Stream 'System.IO.Stream') | Stream. |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | RealmObject-based Class. |

<a name='M-RealmJson-Extensions-RealmDoesJson-CreateOrUpdateObjectFromJson-Realms-Realm,System-String-'></a>
### CreateOrUpdateObjectFromJson(realm,jsonString) `method` [#](#M-RealmJson-Extensions-RealmDoesJson-CreateOrUpdateObjectFromJson-Realms-Realm,System-String- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Creates or updates a single RealmObject from a json string.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| realm | [Realms.Realm](#T-Realms-Realm 'Realms.Realm') | Realm Instance. |
| jsonString | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Json string. |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | RealmObject-based Class. |
