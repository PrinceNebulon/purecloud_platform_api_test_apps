package com.inin;

import com.mypurecloud.sdk.ApiException;
import com.mypurecloud.sdk.ApiResponse;
import com.mypurecloud.sdk.Configuration;
import com.mypurecloud.sdk.api.UsersApi;
import com.mypurecloud.sdk.model.*;
import org.apache.http.Header;

import java.util.Arrays;

public class Main {

    public static void main(String[] args) {
        try {
            Configuration.getDefaultApiClient().setAccessToken("hahanope");
            Configuration.getDefaultApiClient().setShouldThrowErrors(false);
            UsersApi usersApi = new UsersApi();

            ApiResponse<UserMe> me = usersApi.getMeWithHttpInfo(Arrays.asList("presence"));
            if (me.getException() != null) {
                me.getException().printStackTrace();
                return;
            }
            System.out.println("Hello " + me.getResponseObject().getName());
            System.out.println("getRawRequestBody: " + me.getRawRequestBody());
            System.out.println("getRawResponseBody: " + me.getRawResponseBody());
            System.out.println("getHeaders: " + me.getResponseHeaderData());
            System.out.println("getRequestUri: " + me.getRequestUri());
            System.out.println("getResponseCode: " + me.getResponseCode());

            UsersEntityListing users = usersApi.getUsers(25, 1, null, null, Arrays.asList("presence"));
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

            ApiResponse<UsersSearchResponse> userSearchResponseData = usersApi.postSearchWithHttpInfo(searchRequest);

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
        } catch (ApiException e) {
            e.printStackTrace();
        }
    }
}
