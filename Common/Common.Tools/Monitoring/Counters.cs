using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Monitoring
{
    /// <summary>
    /// Don't forget to add the link between your Counter and the Component(s) it refers to
    /// </summary>
	public enum Counters
	{
		#region Common projects

		#region Redis

		Redis_Requests,
		Redis_GetClient,
		Redis_ReleaseClient,

		#endregion

		#region SqlServer

		DataAccess_SqlClient_ExecuteReader,
		DataAccess_SqlClient_ExecuteReaderFailed,
		DataAccess_SqlClient_ExecuteNonQuery,
		DataAccess_SqlClient_ExecuteNonQueryFailed,
		DataAccess_SqlClient_ExecuteScalar,
		DataAccess_SqlClient_ExecuteScalarFailed,

		#endregion

		#region Facebook

		Facebook_GetUserProfile,
		Facebook_GetUserFriends,
		Facebook_GetUserLikes,
		Facebook_GetUserLikesAndProfile,

		#endregion

		#region Neo4j

		Neo4j_QueryBatch,
		Neo4j_InsertBatch,
		Neo4j_InsertDataPlugin,
		Neo4j_InsertDataPluginError,
		Neo4j_ExecuteCypherQuery,
		Neo4j_Pool_GetReaderClientErrors,
		Neo4j_Pool_GetWriterClientErrors,

		Gremlin_ExecuteScript,

		Catalog_UpdateProduct,
		#endregion

		#region MongoClient

		Mongo_Get,
		Mongo_Get_AllDocumentsWithQuery,
		Mongo_Insert,
		Mongo_Update,
		Mongo_Save,
		Mongo_Remove,
		Mongo_GenerateId,

		#endregion

		#endregion

		#region MSMQ

		MSMQ_Send,

		#endregion

		#region Mars Front

		/// <summary>
		/// GetRecommendation client service call -- Platform.Clients.Connectors.RecommendationClient
		/// </summary>
		GetRecommendation_Client,

		/// <summary>
		/// GetRecommendation get random movies for SFR -- DataCenter.ASF.Business.Algo
		/// </summary>
		GetRecommendation_GetRandomMovies,

		/// <summary>
		/// GetRecommendation get the dictionnary of available algo -- DataCenter.ASF.Business.Algo
		/// </summary>
		GetRecommendation_GetAvailableAlgorithms,

		/// <summary>
		/// GetRecommendation get in context the items (mongodb) -- DataCenter.ASF.Business.Algo
		/// </summary>
		GetRecommendation_GetInContext,

		/// <summary>
		/// GetRecommendation client controller call -- SocialReco.Web.Controllers.GetRecommendedItems
		/// </summary>
		GetRecommendation_DisplayBlock,

		/// <summary>
		/// Get the available algorithms -- SocialReco.Business
		/// </summary>
		Recommendations_GetAvailableAlgorithms,

		/// <summary>
		/// Publish Statistics -- SocialReco.Business
		/// </summary>
		Bus_Publish,

		/// <summary>
		/// Get Recommendatio -- SocialReco.Business
		/// </summary>
		Recommendations_GetRecommendation,

		/// <summary>
		/// Get a node by its index -- DataCenter.GraphDatabase
		/// </summary>
		GraphDatabase_GetNodeByIndex,

		/// <summary>
		/// Get all relationship of a type for a node in order to decide the action (create-update) -- DataCenter.GraphDatabase
		/// </summary>
		GraphDatabase_GetAllRelationships,

		/// <summary>
		/// The time taken to create all the graph action in a relationshiphandler -- DataCenter.GraphDatabase
		/// </summary>
		GraphDatabase_RelationshipHandler_GenerateGraphAction,

		/// <summary>
		/// The time taken to execute all the graph action on neo4j (network call included) -- DataCenter.GraphDatabase
		/// </summary>
		GraphDatabase_RelationshipHandler_ExecuteGraphAction,

		/// <summary>
		/// Attach old authUser to a new MarsUser
		/// </summary>
		SocialReco_Business_AddIdentity_ReattachUser,

		/// <summary>
		/// Copy the old relationship to the newly created node
		/// </summary>
		SocialReco_Business_AddIdentity_CopyRelationships,

		/// <summary>
		/// Delete the old MarsUser
		/// </summary>
		SocialReco_Business_AddIdentity_DeleteOriginalMarsUser,

		/// <summary>
		/// Create the user-like relationships synchronously
		/// </summary>
		SocialReco_Business_CreateIdentity_HandleLikes,

		/// <summary>
		/// Create the user node synchronously
		/// </summary>
		SocialReco_Business_CreateIdentity_CreateOnlyNode,

		/// <summary>
		/// Execution of default algorithm
		/// </summary>
		SocialReco_Business_DefaultAlgorithm,

		/// <summary>
		/// Getting random items for quota and filters
		/// </summary>
		DefaultAlgorithm_GetRandomItems,

		/// <summary>
		/// Getting time ordered item for quota and filters
		/// </summary>
		DefaultAlgorithm_GetTimeOrderedItems,

		/// <summary>
		/// Time to build and execute query to get product's data
		/// </summary>
		ProductDataAccess_GetProductInfos,

		/// <summary>
		/// time to get recommendation from Redis cache
		/// </summary>
		RecommendationDataAccess_Get,

		/// <summary>
		/// Getting time ordered item for quota and filters
		/// </summary>
		SocialReco_Business_StandardAlgorithm,

		/// <summary>
		/// Init call
		/// </summary>
		SocialReco_Controller_User_Init,

		/// <summary>
		/// Compute all results and select randomly a goiven number of items
		/// </summary>
		SocialReco_Business_GetRandomFromWeightedList,

		#endregion

		#region Queue Processor

		GraphManagement_ExecuteHandler,

		Recommendation_Statistics_ExecuteMirrorRequest,

		Recommendation_Statistics_InsertStatistics,

		Recommendation_Statistics_SaveStatistics,
		Recommendation_Statistics_ShouldLogChecking,

		UserManagement_UserCreatedHandler,

		UserManagement_InitInactiveUserHandler,

		StandardAlgorithm_Load,
		StandardAlgorithm_LoadAll,

		Message_GenericEvent,

		Analytics_BadgeAdded,
		Analytics_BadgeReceived,
		Analytics_ProductAdded,
		Analytics_ProductDisplayed,
		Analytics_UserCreated,
		Analytics_UserIdentityAdded,
		Analytics_UserRegistered,
		Analytics_UserGotRecommendation,
		Analytics_UserActed,
		Analytics_StoredProcError,
		AnalyticsQueue_PublishError,
		UserManagement_PublishError,
		UserManagement_HandlingLikesError,
		UserManagement_ProcessInactiveUserError,

		#endregion
	}

    public enum Components
    {
        MARS_FRONT,
        EVT_DISP,
        QP,
        MARS_IMPORT
    }

    /// <summary>
    /// This class is used for admin only
    /// You have to add your Component/Platform link whenever you add a new Counter to the enum
    /// </summary>
    public class CounterPlatformLinks
    {
        private Dictionary<Components, List<Counters>> _links = new Dictionary<Components, List<Counters>>();

        #region Singleton

        private static readonly CounterPlatformLinks _instance = new CounterPlatformLinks();

        private CounterPlatformLinks() { Load(); }

        public static CounterPlatformLinks Current { get { return _instance; } }

        #endregion

        public Dictionary<Components, List<Counters>> Links { get { return _links; } }

        private void Load()
        {
			// MARS_IMPORT
			AddLink(Components.MARS_IMPORT, Counters.Catalog_UpdateProduct);

            // MARS_FRONT
            AddLink(Components.MARS_FRONT, Counters.Redis_Requests);
            AddLink(Components.MARS_FRONT, Counters.Redis_GetClient);
            AddLink(Components.MARS_FRONT, Counters.Redis_ReleaseClient);

            AddLink(Components.MARS_FRONT, Counters.DataAccess_SqlClient_ExecuteNonQuery);
            AddLink(Components.MARS_FRONT, Counters.DataAccess_SqlClient_ExecuteNonQueryFailed);
            AddLink(Components.MARS_FRONT, Counters.DataAccess_SqlClient_ExecuteScalar);
            AddLink(Components.MARS_FRONT, Counters.DataAccess_SqlClient_ExecuteScalarFailed);
            AddLink(Components.MARS_FRONT, Counters.DataAccess_SqlClient_ExecuteReader);
            AddLink(Components.MARS_FRONT, Counters.DataAccess_SqlClient_ExecuteReaderFailed);

            AddLink(Components.MARS_FRONT, Counters.Facebook_GetUserProfile);
            AddLink(Components.MARS_FRONT, Counters.Facebook_GetUserFriends);
            AddLink(Components.MARS_FRONT, Counters.Facebook_GetUserLikes);
            AddLink(Components.MARS_FRONT, Counters.Facebook_GetUserLikesAndProfile);

            AddLink(Components.MARS_FRONT, Counters.Neo4j_QueryBatch);
            AddLink(Components.MARS_FRONT, Counters.Neo4j_ExecuteCypherQuery);
            AddLink(Components.MARS_FRONT, Counters.Neo4j_Pool_GetReaderClientErrors);
			AddLink(Components.MARS_FRONT, Counters.Neo4j_Pool_GetWriterClientErrors);
			AddLink(Components.MARS_FRONT, Counters.Neo4j_InsertDataPlugin);
			AddLink(Components.MARS_FRONT, Counters.Neo4j_InsertDataPluginError);

            AddLink(Components.MARS_FRONT, Counters.Mongo_Get);
            AddLink(Components.MARS_FRONT, Counters.Mongo_Get_AllDocumentsWithQuery);
            AddLink(Components.MARS_FRONT, Counters.Mongo_Insert);
            AddLink(Components.MARS_FRONT, Counters.Mongo_Update);
            AddLink(Components.MARS_FRONT, Counters.Mongo_Save);
            AddLink(Components.MARS_FRONT, Counters.Mongo_Remove);
            AddLink(Components.MARS_FRONT, Counters.Mongo_GenerateId);

            AddLink(Components.MARS_FRONT, Counters.MSMQ_Send);

            AddLink(Components.MARS_FRONT, Counters.GetRecommendation_Client);
            AddLink(Components.MARS_FRONT, Counters.GetRecommendation_GetAvailableAlgorithms);
            AddLink(Components.MARS_FRONT, Counters.GetRecommendation_GetRandomMovies);
            AddLink(Components.MARS_FRONT, Counters.GetRecommendation_GetInContext);
            AddLink(Components.MARS_FRONT, Counters.GetRecommendation_DisplayBlock);
            AddLink(Components.MARS_FRONT, Counters.Recommendations_GetRecommendation);
            AddLink(Components.MARS_FRONT, Counters.Recommendations_GetAvailableAlgorithms);
            AddLink(Components.MARS_FRONT, Counters.GraphDatabase_GetNodeByIndex);
            AddLink(Components.MARS_FRONT, Counters.GraphDatabase_GetAllRelationships);
            AddLink(Components.MARS_FRONT, Counters.GraphDatabase_RelationshipHandler_GenerateGraphAction);
            AddLink(Components.MARS_FRONT, Counters.GraphDatabase_RelationshipHandler_ExecuteGraphAction);

            AddLink(Components.MARS_FRONT, Counters.SocialReco_Business_AddIdentity_ReattachUser);
            AddLink(Components.MARS_FRONT, Counters.SocialReco_Business_AddIdentity_CopyRelationships);
            AddLink(Components.MARS_FRONT, Counters.SocialReco_Business_AddIdentity_DeleteOriginalMarsUser);
            AddLink(Components.MARS_FRONT, Counters.SocialReco_Business_CreateIdentity_HandleLikes);
            AddLink(Components.MARS_FRONT, Counters.SocialReco_Business_CreateIdentity_CreateOnlyNode);
            AddLink(Components.MARS_FRONT, Counters.SocialReco_Business_DefaultAlgorithm);
            AddLink(Components.MARS_FRONT, Counters.DefaultAlgorithm_GetRandomItems);
            AddLink(Components.MARS_FRONT, Counters.DefaultAlgorithm_GetTimeOrderedItems);
            AddLink(Components.MARS_FRONT, Counters.ProductDataAccess_GetProductInfos);
            AddLink(Components.MARS_FRONT, Counters.RecommendationDataAccess_Get);
            AddLink(Components.MARS_FRONT, Counters.SocialReco_Business_StandardAlgorithm);

            AddLink(Components.MARS_FRONT, Counters.SocialReco_Controller_User_Init);
            AddLink(Components.MARS_FRONT, Counters.SocialReco_Business_GetRandomFromWeightedList);
            // --------

            // QueueProcessor
            AddLink(Components.QP, Counters.Redis_Requests);
            AddLink(Components.QP, Counters.Redis_GetClient);
            AddLink(Components.QP, Counters.Redis_ReleaseClient);

            AddLink(Components.QP, Counters.DataAccess_SqlClient_ExecuteNonQuery);
            AddLink(Components.QP, Counters.DataAccess_SqlClient_ExecuteNonQueryFailed);
            AddLink(Components.QP, Counters.DataAccess_SqlClient_ExecuteScalar);
            AddLink(Components.QP, Counters.DataAccess_SqlClient_ExecuteScalarFailed);
            AddLink(Components.QP, Counters.DataAccess_SqlClient_ExecuteReader);
            AddLink(Components.QP, Counters.DataAccess_SqlClient_ExecuteReaderFailed);

            AddLink(Components.QP, Counters.Facebook_GetUserProfile);
            AddLink(Components.QP, Counters.Facebook_GetUserFriends);
            AddLink(Components.QP, Counters.Facebook_GetUserLikes);
            AddLink(Components.QP, Counters.Facebook_GetUserLikesAndProfile);

            AddLink(Components.QP, Counters.Neo4j_QueryBatch);
            AddLink(Components.QP, Counters.Neo4j_Pool_GetReaderClientErrors);
            AddLink(Components.QP, Counters.Neo4j_Pool_GetWriterClientErrors);

            AddLink(Components.QP, Counters.Mongo_Get);
            AddLink(Components.QP, Counters.Mongo_Insert);
            AddLink(Components.QP, Counters.Mongo_Update);
            AddLink(Components.QP, Counters.Mongo_Save);
            AddLink(Components.QP, Counters.Mongo_Remove);
            AddLink(Components.QP, Counters.Mongo_GenerateId);

            AddLink(Components.QP, Counters.MSMQ_Send);

            AddLink(Components.QP, Counters.GraphDatabase_GetNodeByIndex);
            AddLink(Components.QP, Counters.GraphDatabase_GetAllRelationships);
            AddLink(Components.QP, Counters.GraphDatabase_RelationshipHandler_GenerateGraphAction);
            AddLink(Components.QP, Counters.GraphDatabase_RelationshipHandler_ExecuteGraphAction);

            AddLink(Components.QP, Counters.Recommendation_Statistics_ExecuteMirrorRequest);
            AddLink(Components.QP, Counters.Recommendation_Statistics_InsertStatistics);
            AddLink(Components.QP, Counters.Recommendation_Statistics_SaveStatistics);
			AddLink(Components.QP, Counters.Recommendation_Statistics_ShouldLogChecking);
            AddLink(Components.QP, Counters.UserManagement_InitInactiveUserHandler);
            AddLink(Components.QP, Counters.UserManagement_UserCreatedHandler);
            AddLink(Components.QP, Counters.GraphManagement_ExecuteHandler);

            AddLink(Components.QP, Counters.UserManagement_PublishError);
            AddLink(Components.QP, Counters.UserManagement_HandlingLikesError);

            AddLink(Components.QP, Counters.StandardAlgorithm_Load);
            AddLink(Components.QP, Counters.StandardAlgorithm_LoadAll);
            AddLink(Components.QP, Counters.Message_GenericEvent);

			AddLink(Components.QP, Counters.Analytics_UserGotRecommendation);
			AddLink(Components.QP, Counters.Analytics_ProductDisplayed);
			AddLink(Components.QP, Counters.Analytics_UserActed);
			AddLink(Components.QP, Counters.Analytics_BadgeReceived);
			AddLink(Components.QP, Counters.Analytics_UserRegistered);
			AddLink(Components.QP, Counters.Analytics_ProductAdded);
			AddLink(Components.QP, Counters.Analytics_BadgeAdded);
			AddLink(Components.QP, Counters.Analytics_UserCreated);
			AddLink(Components.QP, Counters.Analytics_UserIdentityAdded);
			AddLink(Components.QP, Counters.Analytics_StoredProcError);
			AddLink(Components.QP, Counters.AnalyticsQueue_PublishError);

			AddLink(Components.QP, Counters.Neo4j_InsertDataPlugin);
			AddLink(Components.QP, Counters.Neo4j_InsertDataPluginError);
            // error counting


            // --------

            // EventDispatcher
            AddLink(Components.EVT_DISP, Counters.MSMQ_Send);
            // --------

            // MarsImporter
            AddLink(Components.MARS_IMPORT, Counters.Redis_Requests);
            AddLink(Components.MARS_IMPORT, Counters.Redis_GetClient);
            AddLink(Components.MARS_IMPORT, Counters.Redis_ReleaseClient);

            AddLink(Components.MARS_IMPORT, Counters.DataAccess_SqlClient_ExecuteNonQuery);
            AddLink(Components.MARS_IMPORT, Counters.DataAccess_SqlClient_ExecuteNonQueryFailed);
            AddLink(Components.MARS_IMPORT, Counters.DataAccess_SqlClient_ExecuteScalar);
            AddLink(Components.MARS_IMPORT, Counters.DataAccess_SqlClient_ExecuteScalarFailed);
            AddLink(Components.MARS_IMPORT, Counters.DataAccess_SqlClient_ExecuteReader);
            AddLink(Components.MARS_IMPORT, Counters.DataAccess_SqlClient_ExecuteReaderFailed);

            AddLink(Components.MARS_IMPORT, Counters.Facebook_GetUserProfile);
            AddLink(Components.MARS_IMPORT, Counters.Facebook_GetUserFriends);
            AddLink(Components.MARS_IMPORT, Counters.Facebook_GetUserLikes);
            AddLink(Components.MARS_IMPORT, Counters.Facebook_GetUserLikesAndProfile);

            AddLink(Components.MARS_IMPORT, Counters.Neo4j_QueryBatch);
            AddLink(Components.MARS_IMPORT, Counters.Neo4j_Pool_GetReaderClientErrors);
			AddLink(Components.MARS_IMPORT, Counters.Neo4j_Pool_GetWriterClientErrors);
			AddLink(Components.MARS_IMPORT, Counters.Neo4j_InsertDataPlugin);
			AddLink(Components.MARS_IMPORT, Counters.Neo4j_InsertDataPluginError);

            AddLink(Components.MARS_IMPORT, Counters.Mongo_Get);
            AddLink(Components.MARS_IMPORT, Counters.Mongo_Insert);
            AddLink(Components.MARS_IMPORT, Counters.Mongo_Update);
            AddLink(Components.MARS_IMPORT, Counters.Mongo_Save);
            AddLink(Components.MARS_IMPORT, Counters.Mongo_Remove);
            AddLink(Components.MARS_IMPORT, Counters.Mongo_GenerateId);

            AddLink(Components.MARS_IMPORT, Counters.MSMQ_Send);

            AddLink(Components.MARS_IMPORT, Counters.GraphDatabase_GetNodeByIndex);
            AddLink(Components.MARS_IMPORT, Counters.GraphDatabase_GetAllRelationships);
            AddLink(Components.MARS_IMPORT, Counters.GraphDatabase_RelationshipHandler_GenerateGraphAction);
            AddLink(Components.MARS_IMPORT, Counters.GraphDatabase_RelationshipHandler_ExecuteGraphAction);

            AddLink(Components.MARS_IMPORT, Counters.Catalog_UpdateProduct);
        }

        private void AddLink(Components component, Counters counter)
        {
            if (!_links.ContainsKey(component))
            {
                _links[component] = new List<Counters>();
            }
            _links[component].Add(counter);
        }
    }
}
