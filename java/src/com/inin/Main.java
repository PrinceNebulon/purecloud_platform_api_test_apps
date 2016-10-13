package com.inin;

import com.mypurecloud.sdk.ApiException;
import com.mypurecloud.sdk.ApiResponse;
import com.mypurecloud.sdk.Configuration;
import com.mypurecloud.sdk.api.*;
import com.mypurecloud.sdk.model.*;
import org.apache.http.Header;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.Arrays;
import java.util.List;

public class Main {
    public static UsersApi _usersApi = null;
    public static UserMe _me = null;

    public static void main(String[] args) {
        try {
            Configuration.getDefaultApiClient().setAccessToken("access token");
            Configuration.getDefaultApiClient().setShouldThrowErrors(false);
            _usersApi = new UsersApi();

            ApiResponse<UserMe> me = _usersApi.getMeWithHttpInfo(Arrays.asList("presence"));
            if (me.getException() != null) {
                me.getException().printStackTrace();
                return;
            }
            _me = me.getResponseObject();
            System.out.println("Hello " + me.getResponseObject().getName());

            testConversations();

        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public static void createUser() throws ApiException {
        CreateUser body = new CreateUser();
        body.setName("Firstname Børseth");
        body.setDepartment("Sales");
        body.setEmail("hollywoo+nordic2@mydevspace.com");
        body.setTitle("Manager");
        body.setPassword("1234abcdABCD!@#$");

        ApiResponse<User> user = _usersApi.postUsersWithHttpInfo(body);
        System.out.println(user.getException());
        System.out.println("-----------------------");
        System.out.println(user.getRequestHeaderData());
        System.out.println("-----------------------");
        System.out.println(user.getResponseHeaderData());
        System.out.println("-----------------------");
        System.out.println(user.getResponseObject());
        System.out.println("-----------------------");
        System.out.println(user.getRawRequestBody());
    }

    public static void testConversations() throws ApiException, IOException {
        ConversationsApi conversationsApi = new ConversationsApi();

        CreateCallRequest callRequest = new CreateCallRequest();
        callRequest.setPhoneNumber("3172222222");
        CreateCallResponse callResponse = conversationsApi.postCalls(callRequest);
        String callId = callResponse.getId();
        System.out.println("CallId: " + callId);

        BufferedReader br = new BufferedReader(new InputStreamReader(System.in));
        System.out.print("Press enter to put call on hold");
        br.readLine();

        String participantId = "";
        CallConversation conversation = conversationsApi.getCallsCallId(callId);
        for (CallMediaParticipant p : conversation.getParticipants()) {
            if (p.getPurpose() != null && p.getPurpose().equalsIgnoreCase("user")) {
                participantId = p.getId();
            }
        }
        System.out.println("participantId: " + participantId);
        MediaParticipantRequest mediaParticipantRequest = new MediaParticipantRequest();
        mediaParticipantRequest.setHeld(true);
        ApiResponse response = conversationsApi.patchCallsCallIdParticipantsParticipantIdWithHttpInfo(callId, participantId, mediaParticipantRequest);
        System.out.println("response.getCorrelationId() -> " + response.getCorrelationId());
        System.out.println("response.getResponseHeaderData() -> " + response.getResponseHeaderData());
        System.out.println("response.getResponseObject() -> " + response.getResponseObject());
    }

    public static void getStations() throws ApiException {
        StationsApi stationsApi = new StationsApi();
        StationEntityListing stations = stationsApi.getStations(null, null, null, null, null);
        System.out.println(stations);
    }

    public static void updatePresence() throws ApiException, IOException {
        PresenceApi presenceApi = new PresenceApi();
        OrganizationPresenceEntityListing orgPresences = presenceApi.getPresencedefinitions(null, null, null, null);
        OrganizationPresence available = null;
        OrganizationPresence busy = null;
        for (OrganizationPresence presence : orgPresences.getEntities()) {
            if (presence.getSystemPresence().equalsIgnoreCase("available")) {
                available = presence;
            } else if (presence.getSystemPresence().equalsIgnoreCase("busy")) {
                busy = presence;
            }
        }

        BufferedReader br = new BufferedReader(new InputStreamReader(System.in));
        System.out.print("Press enter to set to AVAILABLE");
        br.readLine();

        UserPresence userPresence = new UserPresence();
        userPresence.setPresenceDefinition(available);
        presenceApi.patchUserIdPresencesSourceId(_me.getId(), "PURECLOUD", userPresence);

        br = new BufferedReader(new InputStreamReader(System.in));
        System.out.print("Press enter to set to BUSY");
        br.readLine();

        userPresence = new UserPresence();
        userPresence.setPresenceDefinition(busy);
        presenceApi.patchUserIdPresencesSourceId(_me.getId(), "PURECLOUD", userPresence);
    }

    public static void testEdge() throws ApiException {
        TelephonyProvidersEdgeApi api = new TelephonyProvidersEdgeApi();
        Edge edge = new Edge();
        edge.setName("API Test 1");
        edge.setManaged(false);
        edge = api.postProvidersEdges(edge);
        System.out.println(edge);
    }

    public static void testGetEdge() throws ApiException {
        TelephonyProvidersEdgeApi api = new TelephonyProvidersEdgeApi();
        EdgeEntityListing edges = api.getProvidersEdges(null, null, null, null, null, null);
        System.out.println(edges);
        Edge edge = api.getProvidersEdgesEdgeId(edges.getEntities().get(0).getId());
        System.out.println(edge);
    }

    public static void getUsers() throws ApiException {
        UsersEntityListing users = null;
        users = _usersApi.getUsers(25, 1, null, null, Arrays.asList("presence"));
        System.out.println("\nKnown users:");
        for (User user : users.getEntities()) {
            System.out.println(user.getName());
        }


        UserSearchCriteria criteria = new UserSearchCriteria();
        criteria.setType(UserSearchCriteria.TypeEnum.CONTAINS);
        criteria.setFields(Arrays.asList("name"));
        criteria.value("ho");

        UserSearchRequest searchRequest = new UserSearchRequest();
        searchRequest.setQuery(Arrays.asList(criteria));

        ApiResponse<UsersSearchResponse> userSearchResponseData = _usersApi.postSearchWithHttpInfo(searchRequest);

        System.out.println("\nRequest elements");
        System.out.println("----------------");
        System.out.println("getRequestUri: " + userSearchResponseData.getRequestUri());
        System.out.println("getRawRequestBody: " + userSearchResponseData.getRawRequestBody());
        System.out.println("getRequestHeaderData: " + userSearchResponseData.getRequestHeaderData());

        System.out.println("\nResponse elements");
        System.out.println("-----------------");
        System.out.println("getException: " + userSearchResponseData.getException());
        System.out.println("getResponseCode: " + userSearchResponseData.getResponseCode());
        System.out.println("getRawResponseBody: " + userSearchResponseData.getRawResponseBody());
        System.out.println("getResponseHeaderData: " + userSearchResponseData.getResponseHeaderData());
        System.out.println("getCorrelationId: " + userSearchResponseData.getCorrelationId());

        System.out.println("\nManual header parsing");
        System.out.println("-----------------------");
        for (Header h : userSearchResponseData.getResponseHeaders()) {
            System.out.println("  " + h.getName() + ": " + h.getValue());
        }
    }
}
