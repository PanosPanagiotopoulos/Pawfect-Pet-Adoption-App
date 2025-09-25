import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';
import { QueryResult } from '../common/models/query-result';
import { ConversationLookup } from '../lookup/conversation-lookup';
import {
  Conversation,
  ConversationPersist,
} from '../models/conversation/conversation.model';
import { nameof } from 'ts-simple-nameof';
import { Message } from '../models/message/message.model';
import { User } from '../models/user/user.model';
import { File } from '../models/file/file.model';
import { Shelter } from '../models/shelter/shelter.model';

@Injectable({
  providedIn: 'root',
})
export class ConversationService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.messengerServiceAddress}api/conversations`;
  }

  queryMine(q: ConversationLookup): Observable<QueryResult<Conversation>> {
    const url = `${this.apiBase}/query/mine`;
    return this.http
      .post<QueryResult<Conversation>>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<Conversation> {
    const url = `${this.apiBase}/${id}`;
    let params = new HttpParams();
    reqFields.forEach((field) => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .get<Conversation>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  create(
    item: ConversationPersist,
    reqFields: string[] = []
  ): Observable<Conversation> {
    const url = `${this.apiBase}/create`;
    let params = new HttpParams();
    reqFields.forEach((field) => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .post<Conversation>(url, item, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  delete(id: string): Observable<void> {
    const url = `${this.apiBase}/delete/${id}`;
    return this.http
      .post<void>(url)
      .pipe(catchError((error: any) => throwError(error)));
  }

  static getConversationFields(): string[] {
    return [
      nameof<Conversation>((x) => x.id),
      nameof<Conversation>((x) => x.createdAt),
      nameof<Conversation>((x) => x.lastMessageAt),

      // Last message preview fields
      [
        nameof<Conversation>((x) => x.lastMessagePreview),
        nameof<Message>((x) => x.id),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.lastMessagePreview),
        nameof<Message>((x) => x.content),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.lastMessagePreview),
        nameof<Message>((x) => x.status),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.lastMessagePreview),
        nameof<Message>((x) => x.createdAt),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.lastMessagePreview),
        nameof<Message>((x) => x.updatedAt),
      ].join('.'),

      // Last message sender fields
      [
        nameof<Conversation>((x) => x.lastMessagePreview),
        nameof<Message>((x) => x.sender),
        nameof<User>((x) => x.id),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.lastMessagePreview),
        nameof<Message>((x) => x.sender),
        nameof<User>((x) => x.fullName),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.lastMessagePreview),
        nameof<Message>((x) => x.sender),
        nameof<User>((x) => x.shelter),
        nameof<Shelter>((x) => x.shelterName),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.lastMessagePreview),
        nameof<Message>((x) => x.sender),
        nameof<User>((x) => x.profilePhoto),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),

      // Last message readBy fields
      [
        nameof<Conversation>((x) => x.lastMessagePreview),
        nameof<Message>((x) => x.readBy),
        nameof<User>((x) => x.id),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.lastMessagePreview),
        nameof<Message>((x) => x.readBy),
        nameof<User>((x) => x.fullName),
      ].join('.'),

      // Conversation creator fields
      [
        nameof<Conversation>((x) => x.createdBy),
        nameof<User>((x) => x.id),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.createdBy),
        nameof<User>((x) => x.fullName),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.createdBy),
        nameof<User>((x) => x.shelter),
        nameof<Shelter>((x) => x.shelterName),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.createdBy),
        nameof<User>((x) => x.profilePhoto),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),

      // Participants fields
      [
        nameof<Conversation>((x) => x.participants),
        nameof<User>((x) => x.id),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.participants),
        nameof<User>((x) => x.fullName),
      ].join('.'),
      [
        nameof<Conversation>((x) => x.participants),
        nameof<User>((x) => x.shelter),
        nameof<Shelter>((x) => x.shelterName),  
      ].join('.'),
      [
        nameof<Conversation>((x) => x.participants),
        nameof<User>((x) => x.profilePhoto),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),
    ];
  }
}
